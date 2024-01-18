using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataDashboard.Migrations
{
    /// <inheritdoc />
    public partial class ClientModMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "Sessions",
                newName: "ConnectionId");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConnectedAt",
                table: "Sessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "IP",
                table: "Sessions",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Clients",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectedAt",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "IP",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Clients");

            migrationBuilder.RenameColumn(
                name: "ConnectionId",
                table: "Sessions",
                newName: "SessionId");
        }
    }
}
