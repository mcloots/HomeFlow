using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeFlow.BuildingBlocks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentTypeAndDoneStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "appointments",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "appointments");
        }
    }
}
