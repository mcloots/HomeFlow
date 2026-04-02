using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeFlow.BuildingBlocks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class chores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DueDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Recurrence = table.Column<int>(type: "integer", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByMemberId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chores_HouseholdId_DueDateUtc",
                table: "chores",
                columns: new[] { "HouseholdId", "DueDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_chores_HouseholdId_Status",
                table: "chores",
                columns: new[] { "HouseholdId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chores");
        }
    }
}
