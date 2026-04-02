using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeFlow.BuildingBlocks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class chores_recurrence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecurrenceMonths",
                table: "chores",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecursUntilUtc",
                table: "chores",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecurrenceMonths",
                table: "chores");

            migrationBuilder.DropColumn(
                name: "RecursUntilUtc",
                table: "chores");
        }
    }
}
