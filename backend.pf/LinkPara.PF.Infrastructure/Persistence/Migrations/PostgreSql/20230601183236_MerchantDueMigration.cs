using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MerchantDueMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "posting_bank_balance_id",
                schema: "posting",
                table: "balance",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "merchant_deduction",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_code = table.Column<int>(type: "integer", nullable: false),
                    total_deduction_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    remaining_deduction_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    deduction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    deduction_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_deduction", x => x.id);
                    table.ForeignKey(
                        name: "fk_merchant_deduction_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "merchant_due",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    due_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    total_due_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    remaining_due_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    due_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_due", x => x.id);
                    table.ForeignKey(
                        name: "fk_merchant_due_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_balance_posting_bank_balance_id",
                schema: "posting",
                table: "balance",
                column: "posting_bank_balance_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_deduction_merchant_id",
                schema: "merchant",
                table: "merchant_deduction",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_due_merchant_id",
                schema: "merchant",
                table: "merchant_due",
                column: "merchant_id");

            migrationBuilder.AddForeignKey(
                name: "fk_balance_posting_bank_balance_posting_bank_balance_id",
                schema: "posting",
                table: "balance",
                column: "posting_bank_balance_id",
                principalSchema: "posting",
                principalTable: "bank_balance",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_balance_posting_bank_balance_posting_bank_balance_id",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropTable(
                name: "merchant_deduction",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "merchant_due",
                schema: "merchant");

            migrationBuilder.DropIndex(
                name: "ix_balance_posting_bank_balance_id",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "posting_bank_balance_id",
                schema: "posting",
                table: "balance");
        }
    }
}
