using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PaymentToWalletMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "posting_payment_channel",
                schema: "merchant",
                table: "pool",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "BankAccount");

            migrationBuilder.AddColumn<string>(
                name: "wallet_number",
                schema: "merchant",
                table: "pool",
                type: "character varying(26)",
                maxLength: 26,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "posting_payment_channel",
                schema: "merchant",
                table: "merchant",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "BankAccount");

            migrationBuilder.AddColumn<string>(
                name: "posting_payment_channel",
                schema: "posting",
                table: "balance",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "wallet_number",
                schema: "posting",
                table: "balance",
                type: "character varying(26)",
                maxLength: 26,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "wallet",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_number = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet", x => x.id);
                    table.ForeignKey(
                        name: "fk_wallet_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_wallet_merchant_id",
                schema: "merchant",
                table: "wallet",
                column: "merchant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wallet",
                schema: "merchant");

            migrationBuilder.DropColumn(
                name: "posting_payment_channel",
                schema: "merchant",
                table: "pool");

            migrationBuilder.DropColumn(
                name: "wallet_number",
                schema: "merchant",
                table: "pool");

            migrationBuilder.DropColumn(
                name: "posting_payment_channel",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "posting_payment_channel",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "wallet_number",
                schema: "posting",
                table: "balance");
        }
    }
}
