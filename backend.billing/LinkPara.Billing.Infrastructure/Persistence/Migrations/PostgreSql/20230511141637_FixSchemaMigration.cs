using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Billing.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class FixSchemaMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reconciliation");

            migrationBuilder.RenameTable(
                name: "summary",
                schema: "core",
                newName: "summary",
                newSchema: "reconciliation");

            migrationBuilder.RenameTable(
                name: "institution_summary",
                schema: "core",
                newName: "institution_summary",
                newSchema: "reconciliation");

            migrationBuilder.RenameTable(
                name: "institution_detail",
                schema: "core",
                newName: "institution_detail",
                newSchema: "reconciliation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "summary",
                schema: "reconciliation",
                newName: "summary",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "institution_summary",
                schema: "reconciliation",
                newName: "institution_summary",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "institution_detail",
                schema: "reconciliation",
                newName: "institution_detail",
                newSchema: "core");
        }
    }
}
