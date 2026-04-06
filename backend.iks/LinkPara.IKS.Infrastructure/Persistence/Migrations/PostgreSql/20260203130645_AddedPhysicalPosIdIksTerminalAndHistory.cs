using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.IKS.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedPhysicalPosIdIksTerminalAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "physical_pos_id",
                schema: "core",
                table: "iks_terminal_history",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "physical_pos_id",
                schema: "core",
                table: "iks_terminal",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "physical_pos_id",
                schema: "core",
                table: "iks_terminal_history");

            migrationBuilder.DropColumn(
                name: "physical_pos_id",
                schema: "core",
                table: "iks_terminal");
        }
    }
}
