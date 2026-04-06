using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class UpdateRestrictionsForIbanAndBankCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_pool_bank_bank_code",
                schema: "merchant",
                table: "pool");

            migrationBuilder.DropIndex(
                name: "ix_pool_bank_code",
                schema: "merchant",
                table: "pool");

            migrationBuilder.AlterColumn<string>(
                name: "iban",
                schema: "merchant",
                table: "pool",
                type: "character varying(26)",
                maxLength: 26,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(26)",
                oldMaxLength: 26);

            migrationBuilder.AlterColumn<string>(
                name: "bank_name",
                schema: "merchant",
                table: "merchant_return_pool",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "iban",
                schema: "merchant",
                table: "pool",
                type: "character varying(26)",
                maxLength: 26,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(26)",
                oldMaxLength: 26,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "bank_name",
                schema: "merchant",
                table: "merchant_return_pool",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_pool_bank_code",
                schema: "merchant",
                table: "pool",
                column: "bank_code");

            migrationBuilder.AddForeignKey(
                name: "fk_pool_bank_bank_code",
                schema: "merchant",
                table: "pool",
                column: "bank_code",
                principalSchema: "bank",
                principalTable: "bank",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
