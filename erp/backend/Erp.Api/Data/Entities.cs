namespace Erp.Api.Data;

public enum UserRole
{
    Admin = 1,
    Supervisor = 2,
    Worker = 3
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int? SupervisorRank { get; set; } // only for supervisors (1 or 2)

    public ICollection<Timesheet> Timesheets { get; set; } = new List<Timesheet>();
}

public enum TimesheetStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Declined = 3
}

public class Timesheet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public int Year { get; set; }
    public int Week { get; set; }
    public TimesheetStatus Status { get; set; } = TimesheetStatus.Draft;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<TimesheetRow> Rows { get; set; } = new List<TimesheetRow>();
}

public class TimesheetRow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TimesheetId { get; set; }
    public Timesheet Timesheet { get; set; } = null!;

    public string ProjectOrTask { get; set; } = string.Empty;
    public string? Notes { get; set; }

    // Hours per weekday (Mon..Sun)
    public decimal Monday { get; set; }
    public decimal Tuesday { get; set; }
    public decimal Wednesday { get; set; }
    public decimal Thursday { get; set; }
    public decimal Friday { get; set; }
    public decimal Saturday { get; set; }
    public decimal Sunday { get; set; }
}