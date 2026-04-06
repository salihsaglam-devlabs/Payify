using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PostingStatementMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "posting_date",
                schema: "posting",
                table: "item",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.CreateTable(
                name: "posting_statement",
                schema: "posting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_pf_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_pf_net_commission_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_due_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_posting_statement", x => x.id);
                    table.ForeignKey(
                        name: "fk_posting_statement_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_item_merchant_id",
                schema: "posting",
                table: "item",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_merchant_id_posting_date",
                schema: "posting",
                table: "item",
                columns: new[] { "merchant_id", "posting_date" });

            migrationBuilder.CreateIndex(
                name: "ix_posting_statement_merchant_id",
                schema: "posting",
                table: "posting_statement",
                column: "merchant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "posting_statement",
                schema: "posting");

            migrationBuilder.DropIndex(
                name: "ix_item_merchant_id",
                schema: "posting",
                table: "item");

            migrationBuilder.DropIndex(
                name: "ix_item_merchant_id_posting_date",
                schema: "posting",
                table: "item");

            migrationBuilder.AlterColumn<DateTime>(
                name: "posting_date",
                schema: "posting",
                table: "item",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");
        }
    }
}
