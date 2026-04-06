using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedParentMerchantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_invoice_commission_reflected",
                schema: "merchant",
                table: "pool",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "parent_merchant_id",
                schema: "merchant",
                table: "pool",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "parent_merchant_name",
                schema: "merchant",
                table: "pool",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "parent_merchant_number",
                schema: "merchant",
                table: "pool",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_invoice_commission_reflected",
                schema: "merchant",
                table: "merchant",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "parent_merchant_id",
                schema: "merchant",
                table: "merchant",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "parent_merchant_name",
                schema: "merchant",
                table: "merchant",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "parent_merchant_number",
                schema: "merchant",
                table: "merchant",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_invoice_commission_reflected",
                schema: "merchant",
                table: "pool");

            migrationBuilder.DropColumn(
                name: "parent_merchant_id",
                schema: "merchant",
                table: "pool");

            migrationBuilder.DropColumn(
                name: "parent_merchant_name",
                schema: "merchant",
                table: "pool");

            migrationBuilder.DropColumn(
                name: "parent_merchant_number",
                schema: "merchant",
                table: "pool");

            migrationBuilder.DropColumn(
                name: "is_invoice_commission_reflected",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "parent_merchant_id",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "parent_merchant_name",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "parent_merchant_number",
                schema: "merchant",
                table: "merchant");
        }
    }
}
