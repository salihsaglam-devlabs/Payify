using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostingTransactionNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "easy_sub_merchant_name",
                schema: "posting",
                table: "transaction",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "easy_sub_merchant_number",
                schema: "posting",
                table: "transaction",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "merchant_deduction_id",
                schema: "posting",
                table: "transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "related_posting_balance_id",
                schema: "posting",
                table: "transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "sub_merchant_id",
                schema: "posting",
                table: "transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "sub_merchant_name",
                schema: "posting",
                table: "transaction",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sub_merchant_number",
                schema: "posting",
                table: "transaction",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "easy_sub_merchant_name",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "easy_sub_merchant_number",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "easy_sub_merchant_name",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "easy_sub_merchant_number",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "merchant_deduction_id",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "related_posting_balance_id",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "sub_merchant_id",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "sub_merchant_name",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "sub_merchant_number",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "easy_sub_merchant_name",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "easy_sub_merchant_number",
                schema: "posting",
                table: "posting_additional_transaction");
        }
    }
}
