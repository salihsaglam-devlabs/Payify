using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class CompanyPoolIbanRequiredDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "iban",
                schema: "core",
                table: "company_pool",
                type: "character varying(26)",
                maxLength: 26,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(26)",
                oldMaxLength: 26);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "iban",
                schema: "core",
                table: "company_pool",
                type: "character varying(26)",
                maxLength: 26,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(26)",
                oldMaxLength: 26,
                oldNullable: true);
        }
    }
}
