using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class BulkTransferTablesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "order_number",
                schema: "core",
                table: "card_topup_request",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "bulk_transfer",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bulk_transfer_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_name = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    reference_number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    action_user = table.Column<Guid>(type: "uuid", nullable: true),
                    action_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    sender_wallet_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bulk_transfer_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bulk_transfer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bulk_transfer_detail",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bulk_transfer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    description = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    receiver = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bulk_transfer_detail_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bulk_transfer_detail", x => x.id);
                    table.ForeignKey(
                        name: "fk_bulk_transfer_detail_bulk_transfer_bulk_transfer_id",
                        column: x => x.bulk_transfer_id,
                        principalSchema: "core",
                        principalTable: "bulk_transfer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_bulk_transfer_detail_transaction_transaction_id",
                        column: x => x.transaction_id,
                        principalSchema: "core",
                        principalTable: "transaction",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_bulk_transfer_detail_bulk_transfer_id",
                schema: "core",
                table: "bulk_transfer_detail",
                column: "bulk_transfer_id");

            migrationBuilder.CreateIndex(
                name: "ix_bulk_transfer_detail_transaction_id",
                schema: "core",
                table: "bulk_transfer_detail",
                column: "transaction_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bulk_transfer_detail",
                schema: "core");

            migrationBuilder.DropTable(
                name: "bulk_transfer",
                schema: "core");

            migrationBuilder.AlterColumn<string>(
                name: "order_number",
                schema: "core",
                table: "card_topup_request",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
