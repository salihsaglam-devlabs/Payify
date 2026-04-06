using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Fraud.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedComplianceRuleSetKeyField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "compliance_rule_set_key",
                schema: "core",
                table: "triggered_rule_set_key",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "compliance_rule_set_key",
                schema: "core",
                table: "triggered_rule_set_key");
        }
    }
}
