using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PaxDeviceTerminalStatusUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "device_terminal_last_activity",
                schema: "merchant",
                table: "merchant_physical_pos",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "device_terminal_status",
                schema: "merchant",
                table: "merchant_physical_pos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Unknown");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "device_terminal_last_activity",
                schema: "merchant",
                table: "merchant_physical_pos");

            migrationBuilder.DropColumn(
                name: "device_terminal_status",
                schema: "merchant",
                table: "merchant_physical_pos");
        }
    }
}
