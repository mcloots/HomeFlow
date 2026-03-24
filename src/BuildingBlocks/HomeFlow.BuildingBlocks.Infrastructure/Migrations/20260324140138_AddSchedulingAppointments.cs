using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeFlow.BuildingBlocks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulingAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StartsAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "appointment_participants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment_participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_appointment_participants_appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointment_participants_AppointmentId_HouseholdMemberId",
                table: "appointment_participants",
                columns: new[] { "AppointmentId", "HouseholdMemberId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appointment_participants");

            migrationBuilder.DropTable(
                name: "appointments");
        }
    }
}
