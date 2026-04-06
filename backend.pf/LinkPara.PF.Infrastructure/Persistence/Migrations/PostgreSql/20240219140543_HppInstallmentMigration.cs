using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class HppInstallmentMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "amount",
                schema: "hpp",
                table: "hosted_payment_installment",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "card_network",
                schema: "hpp",
                table: "hosted_payment_installment",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "enable_installments",
                schema: "hpp",
                table: "hosted_payment",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount",
                schema: "hpp",
                table: "hosted_payment_installment");

            migrationBuilder.DropColumn(
                name: "card_network",
                schema: "hpp",
                table: "hosted_payment_installment");

            migrationBuilder.DropColumn(
                name: "enable_installments",
                schema: "hpp",
                table: "hosted_payment");
        }
    }
}
