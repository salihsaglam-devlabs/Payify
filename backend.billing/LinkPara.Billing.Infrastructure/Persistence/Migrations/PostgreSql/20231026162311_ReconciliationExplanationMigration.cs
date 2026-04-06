using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Billing.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class ReconciliationExplanationMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "explanation",
                schema: "reconciliation",
                table: "summary",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "explanation",
                schema: "reconciliation",
                table: "summary");
        }
    }
}
