using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MerchantDueTypeMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "due_type",
                schema: "merchant",
                table: "merchant_due");

            migrationBuilder.RenameColumn(
                name: "execution_date",
                schema: "merchant",
                table: "merchant_due",
                newName: "last_execution_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "last_execution_date",
                schema: "merchant",
                table: "merchant_due",
                newName: "execution_date");

            migrationBuilder.AddColumn<string>(
                name: "due_type",
                schema: "merchant",
                table: "merchant_due",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
