using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MerchantStatementUpdatePhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_success",
                schema: "merchant",
                table: "merchant_statement");

            migrationBuilder.AlterColumn<string>(
                name: "file_path",
                schema: "merchant",
                table: "merchant_statement",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "file_name",
                schema: "merchant",
                table: "merchant_statement",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "merchant",
                table: "merchant_statement",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "receipt_number",
                schema: "merchant",
                table: "merchant_statement",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "statement_status",
                schema: "merchant",
                table: "merchant_statement",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "statement_type",
                schema: "merchant",
                table: "merchant_statement",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                schema: "merchant",
                table: "merchant_statement");

            migrationBuilder.DropColumn(
                name: "receipt_number",
                schema: "merchant",
                table: "merchant_statement");

            migrationBuilder.DropColumn(
                name: "statement_status",
                schema: "merchant",
                table: "merchant_statement");

            migrationBuilder.DropColumn(
                name: "statement_type",
                schema: "merchant",
                table: "merchant_statement");

            migrationBuilder.AlterColumn<string>(
                name: "file_path",
                schema: "merchant",
                table: "merchant_statement",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "file_name",
                schema: "merchant",
                table: "merchant_statement",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_success",
                schema: "merchant",
                table: "merchant_statement",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
