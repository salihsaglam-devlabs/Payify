using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MerchantStatementUpdatePhase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "file_path",
                schema: "merchant",
                table: "merchant_statement",
                newName: "pdf_path");

            migrationBuilder.AddColumn<string>(
                name: "excel_path",
                schema: "merchant",
                table: "merchant_statement",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "excel_path",
                schema: "merchant",
                table: "merchant_statement");

            migrationBuilder.RenameColumn(
                name: "pdf_path",
                schema: "merchant",
                table: "merchant_statement",
                newName: "file_path");
        }
    }
}
