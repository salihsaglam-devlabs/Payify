using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostingNewDeductionFieldsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "total_submerchant_deduction_amount",
                schema: "posting",
                table: "balance",
                newName: "total_suspicious_transfer_amount");

            migrationBuilder.AddColumn<decimal>(
                name: "total_chargeback_commission_amount",
                schema: "posting",
                table: "balance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_chargeback_transfer_amount",
                schema: "posting",
                table: "balance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_due_transfer_amount",
                schema: "posting",
                table: "balance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_excess_return_on_commission_amount",
                schema: "posting",
                table: "balance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_excess_return_transfer_amount",
                schema: "posting",
                table: "balance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_suspicious_commission_amount",
                schema: "posting",
                table: "balance",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "total_chargeback_commission_amount",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "total_chargeback_transfer_amount",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "total_due_transfer_amount",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "total_excess_return_on_commission_amount",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "total_excess_return_transfer_amount",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "total_suspicious_commission_amount",
                schema: "posting",
                table: "balance");

            migrationBuilder.RenameColumn(
                name: "total_suspicious_transfer_amount",
                schema: "posting",
                table: "balance",
                newName: "total_submerchant_deduction_amount");
        }
    }
}
