using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class HppTransactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hosted_payment_transaction",
                schema: "hpp",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tracking_id = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    hpp_payment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_id = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    installment_count = table.Column<int>(type: "integer", nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    is3d_required = table.Column<bool>(type: "boolean", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hosted_payment_transaction", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_hosted_payment_transaction_tracking_id",
                schema: "hpp",
                table: "hosted_payment_transaction",
                column: "tracking_id");

            migrationBuilder.CreateIndex(
                name: "ix_hosted_payment_transaction_transaction_date",
                schema: "hpp",
                table: "hosted_payment_transaction",
                column: "transaction_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hosted_payment_transaction",
                schema: "hpp");
        }
    }
}
