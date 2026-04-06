using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostingMoneyTransferMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "transaction_type",
                schema: "posting",
                table: "bank_balance");

            migrationBuilder.DropColumn(
                name: "transaction_type",
                schema: "posting",
                table: "balance");

            migrationBuilder.AddColumn<DateTime>(
                name: "transaction_date",
                schema: "posting",
                table: "bank_balance",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "iban",
                schema: "posting",
                table: "balance",
                type: "character varying(26)",
                maxLength: 26,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "money_transfer_bank_code",
                schema: "posting",
                table: "balance",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "money_transfer_bank_name",
                schema: "posting",
                table: "balance",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "transaction_source_id",
                schema: "posting",
                table: "balance",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "transaction_date",
                schema: "posting",
                table: "bank_balance");

            migrationBuilder.DropColumn(
                name: "iban",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "money_transfer_bank_code",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "money_transfer_bank_name",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "transaction_source_id",
                schema: "posting",
                table: "balance");

            migrationBuilder.AddColumn<string>(
                name: "transaction_type",
                schema: "posting",
                table: "bank_balance",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "transaction_type",
                schema: "posting",
                table: "balance",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
