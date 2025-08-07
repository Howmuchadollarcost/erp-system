using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Erp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    SupervisorRank = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Timesheets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Week = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timesheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Timesheets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimesheetRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimesheetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectOrTask = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Monday = table.Column<decimal>(type: "TEXT", nullable: false),
                    Tuesday = table.Column<decimal>(type: "TEXT", nullable: false),
                    Wednesday = table.Column<decimal>(type: "TEXT", nullable: false),
                    Thursday = table.Column<decimal>(type: "TEXT", nullable: false),
                    Friday = table.Column<decimal>(type: "TEXT", nullable: false),
                    Saturday = table.Column<decimal>(type: "TEXT", nullable: false),
                    Sunday = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimesheetRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimesheetRows_Timesheets_TimesheetId",
                        column: x => x.TimesheetId,
                        principalTable: "Timesheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetRows_TimesheetId",
                table: "TimesheetRows",
                column: "TimesheetId");

            migrationBuilder.CreateIndex(
                name: "IX_Timesheets_UserId_Year_Week",
                table: "Timesheets",
                columns: new[] { "UserId", "Year", "Week" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimesheetRows");

            migrationBuilder.DropTable(
                name: "Timesheets");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
