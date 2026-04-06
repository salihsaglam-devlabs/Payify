using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedPostingPfProfitTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "bank_payment_date",
                schema: "posting",
                table: "transaction",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "pf_profit",
                schema: "posting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_date = table.Column<DateTime>(type: "date", nullable: false),
                    amount_without_bank_commission = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_paying_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_pf_net_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    protection_transfer_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pf_profit", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pf_profit_detail",
                schema: "posting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    acquire_bank_code = table.Column<int>(type: "integer", nullable: false),
                    bank_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    bank_paying_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    posting_pf_profit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pf_profit_detail", x => x.id);
                    table.ForeignKey(
                        name: "fk_pf_profit_detail_pf_profit_posting_pf_profit_id",
                        column: x => x.posting_pf_profit_id,
                        principalSchema: "posting",
                        principalTable: "pf_profit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pf_profit_detail_posting_pf_profit_id",
                schema: "posting",
                table: "pf_profit_detail",
                column: "posting_pf_profit_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pf_profit_detail",
                schema: "posting");

            migrationBuilder.DropTable(
                name: "pf_profit",
                schema: "posting");

            migrationBuilder.DropColumn(
                name: "bank_payment_date",
                schema: "posting",
                table: "transaction");
        }
    }
}
