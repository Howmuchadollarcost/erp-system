using Erp.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Erp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimesheetsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TimesheetsController(AppDbContext db)
    {
        _db = db;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private string GetRole() => User.FindFirstValue(ClaimTypes.Role)!;
    private int? GetRank() { var r = User.FindFirstValue("rank"); return int.TryParse(r, out var v) ? v : null; }

    public record TimesheetDto(Guid Id, int Year, int Week, TimesheetStatus Status, List<TimesheetRowDto> Rows);
    public record TimesheetRowDto(Guid Id, string ProjectOrTask, string? Notes,
        decimal Monday, decimal Tuesday, decimal Wednesday, decimal Thursday, decimal Friday, decimal Saturday, decimal Sunday);

    public record TimesheetWithUserDto(Guid Id, int Year, int Week, TimesheetStatus Status, string Username);

    private static TimesheetDto ToDto(Timesheet t) => new(
        t.Id, t.Year, t.Week, t.Status,
        t.Rows.Select(r => new TimesheetRowDto(r.Id, r.ProjectOrTask, r.Notes, r.Monday, r.Tuesday, r.Wednesday, r.Thursday, r.Friday, r.Saturday, r.Sunday)).ToList());

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<TimesheetDto>>> GetMyTimesheets([FromQuery] int? year, [FromQuery] int? week)
    {
        var uid = GetUserId();
        var q = _db.Timesheets.Include(t => t.Rows).Where(t => t.UserId == uid).AsQueryable();
        if (year.HasValue) q = q.Where(t => t.Year == year);
        if (week.HasValue) q = q.Where(t => t.Week == week);
        var list = await q.OrderByDescending(t => t.Year).ThenByDescending(t => t.Week).ToListAsync();
        return list.Select(ToDto).ToList();
    }

    public record UpsertTimesheetRequest(int Year, int Week, List<UpsertRow> Rows, TimesheetStatus? Status);
    public record UpsertRow(Guid? Id, string ProjectOrTask, string? Notes,
        decimal Monday, decimal Tuesday, decimal Wednesday, decimal Thursday, decimal Friday, decimal Saturday, decimal Sunday);

    [Authorize(Policy = "WorkerOnly")]
    [HttpPost("upsert")]
    public async Task<ActionResult<TimesheetDto>> UpsertMine(UpsertTimesheetRequest request)
    {
        var uid = GetUserId();
        var ts = await _db.Timesheets.Include(t => t.Rows)
            .FirstOrDefaultAsync(t => t.UserId == uid && t.Year == request.Year && t.Week == request.Week);
        if (ts == null)
        {
            ts = new Timesheet { UserId = uid, Year = request.Year, Week = request.Week, Status = TimesheetStatus.Draft };
            _db.Timesheets.Add(ts);
        }
        if (ts.Status == TimesheetStatus.Approved || ts.Status == TimesheetStatus.Submitted)
        {
            if (ts.Status == TimesheetStatus.Approved)
                return BadRequest("Approved timesheet cannot be edited");
        }
        _db.TimesheetRows.RemoveRange(ts.Rows);
        ts.Rows = request.Rows.Select(r => new TimesheetRow
        {
            ProjectOrTask = r.ProjectOrTask,
            Notes = r.Notes,
            Monday = r.Monday,
            Tuesday = r.Tuesday,
            Wednesday = r.Wednesday,
            Thursday = r.Thursday,
            Friday = r.Friday,
            Saturday = r.Saturday,
            Sunday = r.Sunday
        }).ToList();
        if (request.Status.HasValue) ts.Status = request.Status.Value;
        ts.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(ts);
    }

    [Authorize]
    [HttpPost("submit")]
    public async Task<ActionResult<TimesheetDto>> SubmitMine([FromBody] int year, [FromQuery] int week)
    {
        var uid = GetUserId();
        var ts = await _db.Timesheets.Include(t => t.Rows).FirstOrDefaultAsync(t => t.UserId == uid && t.Year == year && t.Week == week);
        if (ts == null) return NotFound();
        ts.Status = TimesheetStatus.Submitted;
        ts.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(ts);
    }

    // Supervisor/admin review
    [Authorize(Policy = "SupervisorOnly")]
    [HttpGet("review")] // query by username optional
    public async Task<ActionResult<IEnumerable<TimesheetWithUserDto>>> Review([FromQuery] string? username, [FromQuery] int? year, [FromQuery] int? week)
    {
        var myRank = GetRank() ?? 1; // default 1
        var q = _db.Timesheets.Include(t => t.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(username)) q = q.Where(t => t.User.Username.Contains(username));
        if (year.HasValue) q = q.Where(t => t.Year == year);
        if (week.HasValue) q = q.Where(t => t.Week == week);

        // Supervisors can see workers and supervisors with lower rank
        q = q.Where(t => t.User.Role == UserRole.Worker || (t.User.Role == UserRole.Supervisor && (t.User.SupervisorRank ?? 0) < myRank));

        var list = await q.OrderByDescending(t => t.Year).ThenByDescending(t => t.Week)
            .Select(t => new TimesheetWithUserDto(t.Id, t.Year, t.Week, t.Status, t.User.Username))
            .ToListAsync();
        return list;
    }

    public record ReviewActionRequest(Guid TimesheetId, bool Approve, string? Reason);

    [Authorize(Policy = "SupervisorOnly")]
    [HttpPost("review/action")]
    public async Task<ActionResult<TimesheetDto>> ReviewAction(ReviewActionRequest request)
    {
        var myRank = GetRank() ?? 1;
        var ts = await _db.Timesheets.Include(t => t.User).Include(t => t.Rows).FirstOrDefaultAsync(t => t.Id == request.TimesheetId);
        if (ts == null) return NotFound();
        // validate rank
        if (ts.User.Role == UserRole.Supervisor && ((ts.User.SupervisorRank ?? 0) >= myRank))
        {
            return Forbid();
        }
        if (request.Approve)
            ts.Status = TimesheetStatus.Approved;
        else
            ts.Status = TimesheetStatus.Declined;
        ts.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(ts);
    }

    // Admin override
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("admin/override")] // set any status
    public async Task<ActionResult<TimesheetDto>> AdminOverride([FromBody] Guid timesheetId, [FromQuery] TimesheetStatus status)
    {
        var ts = await _db.Timesheets.Include(t => t.Rows).FirstOrDefaultAsync(t => t.Id == timesheetId);
        if (ts == null) return NotFound();
        ts.Status = status;
        ts.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(ts);
    }
}