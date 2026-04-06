using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Identity.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class ScreenFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "icon",
                schema: "core",
                table: "screen",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "link",
                schema: "core",
                table: "screen",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "module_icon",
                schema: "core",
                table: "screen",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "module_link",
                schema: "core",
                table: "screen",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "module_priorty",
                schema: "core",
                table: "screen",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "priorty",
                schema: "core",
                table: "screen",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "icon",
                schema: "core",
                table: "screen");

            migrationBuilder.DropColumn(
                name: "link",
                schema: "core",
                table: "screen");

            migrationBuilder.DropColumn(
                name: "module_icon",
                schema: "core",
                table: "screen");

            migrationBuilder.DropColumn(
                name: "module_link",
                schema: "core",
                table: "screen");

            migrationBuilder.DropColumn(
                name: "module_priorty",
                schema: "core",
                table: "screen");

            migrationBuilder.DropColumn(
                name: "priorty",
                schema: "core",
                table: "screen");
        }
    }
}
