using BCrypt.Net;
using Erp.Api.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Erp.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db, SecurityKey signingKey, JwtSettings jwt)
    {
        await db.Database.EnsureCreatedAsync();

        if (!await db.Users.AnyAsync())
        {
            var admin = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = UserRole.Admin
            };
            var sup1 = new User
            {
                Username = "super1",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("super123"),
                Role = UserRole.Supervisor,
                SupervisorRank = 1
            };
            var sup2 = new User
            {
                Username = "super2",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("super123"),
                Role = UserRole.Supervisor,
                SupervisorRank = 2
            };
            var worker = new User
            {
                Username = "worker",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("worker123"),
                Role = UserRole.Worker
            };

            db.Users.AddRange(admin, sup1, sup2, worker);
            await db.SaveChangesAsync();

            // Seed a sample timesheet for worker current week
            var now = DateTime.UtcNow;
            var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            var week = calendar.GetWeekOfYear(now, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var ts = new Timesheet
            {
                UserId = worker.Id,
                Year = now.Year,
                Week = week,
                Status = TimesheetStatus.Draft,
                Rows = new List<TimesheetRow>
                {
                    new TimesheetRow
                    {
                        ProjectOrTask = "Onboarding",
                        Notes = "Initial setup",
                        Monday = 2, Tuesday = 2, Wednesday = 2, Thursday = 2, Friday = 2
                    }
                }
            };
            db.Timesheets.Add(ts);
            await db.SaveChangesAsync();
        }
    }
}