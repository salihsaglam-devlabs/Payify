using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PerInstallmentPostingUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_transaction_merchant_transaction_id",
                schema: "posting",
                table: "transaction");

            migrationBuilder.AddColumn<int>(
                name: "installment_sequence",
                schema: "posting",
                table: "transaction",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_per_installment",
                schema: "posting",
                table: "transaction",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "merchant_installment_transaction_id",
                schema: "posting",
                table: "transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "installment_sequence",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_per_installment",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "merchant_installment_transaction_id",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_transaction_merchant_transaction_id_merchant_installment_tr",
                schema: "posting",
                table: "transaction",
                columns: new[] { "merchant_transaction_id", "merchant_installment_transaction_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_transaction_merchant_transaction_id_merchant_installment_tr",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "installment_sequence",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "is_per_installment",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "merchant_installment_transaction_id",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "installment_sequence",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "is_per_installment",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "merchant_installment_transaction_id",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_merchant_transaction_id",
                schema: "posting",
                table: "transaction",
                column: "merchant_transaction_id",
                unique: true);
        }
    }
}
