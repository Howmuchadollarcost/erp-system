using Microsoft.EntityFrameworkCore;

namespace Erp.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();
    public DbSet<TimesheetRow> TimesheetRows => Set<TimesheetRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Username).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Timesheet>(e =>
        {
            e.HasIndex(t => new { t.UserId, t.Year, t.Week }).IsUnique();
            e.HasMany(t => t.Rows)
             .WithOne(r => r.Timesheet)
             .HasForeignKey(r => r.TimesheetId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TimesheetRow>(e =>
        {
            e.Property(r => r.ProjectOrTask).IsRequired();
        });
    }
}