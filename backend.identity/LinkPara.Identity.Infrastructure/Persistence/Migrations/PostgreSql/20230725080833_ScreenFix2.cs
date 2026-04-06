using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Identity.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class ScreenFix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "priorty",
                schema: "core",
                table: "screen",
                newName: "priority");

            migrationBuilder.RenameColumn(
                name: "module_priorty",
                schema: "core",
                table: "screen",
                newName: "module_priority");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "priority",
                schema: "core",
                table: "screen",
                newName: "priorty");

            migrationBuilder.RenameColumn(
                name: "module_priority",
                schema: "core",
                table: "screen",
                newName: "module_priorty");
        }
    }
}
