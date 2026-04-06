using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class OnUsPaymentInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_on_us_vpos",
                schema: "vpos",
                table: "vpos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "merchant_customer_phone_number",
                schema: "merchant",
                table: "transaction",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_on_us_payment",
                schema: "merchant",
                table: "transaction",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "merchant_customer_phone_code",
                schema: "merchant",
                table: "transaction",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "md",
                schema: "core",
                table: "three_d_verification",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_on_us_vpos",
                schema: "vpos",
                table: "vpos");

            migrationBuilder.DropColumn(
                name: "is_on_us_payment",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "merchant_customer_phone_code",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.AlterColumn<string>(
                name: "merchant_customer_phone_number",
                schema: "merchant",
                table: "transaction",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "md",
                schema: "core",
                table: "three_d_verification",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);
        }
    }
}
