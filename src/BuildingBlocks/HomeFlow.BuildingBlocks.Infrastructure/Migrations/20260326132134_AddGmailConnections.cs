using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeFlow.BuildingBlocks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGmailConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gmail_connections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoogleEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    EncryptedRefreshToken = table.Column<string>(type: "text", nullable: false),
                    EncryptedAccessToken = table.Column<string>(type: "text", nullable: true),
                    Scopes = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ConnectedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gmail_connections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gmail_connections_HouseholdId_Status",
                table: "gmail_connections",
                columns: new[] { "HouseholdId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gmail_connections");
        }
    }
}
