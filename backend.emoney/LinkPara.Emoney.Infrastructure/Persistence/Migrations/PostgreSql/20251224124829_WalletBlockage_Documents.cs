using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class WalletBlockage_Documents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "wallet_blockage",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    account_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    wallet_currency_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    cash_blockage_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    credit_blockage_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    operation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    blockage_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    blockage_type = table.Column<string>(type: "text", nullable: true),
                    blockage_description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    blockage_start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    blockage_end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet_blockage", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wallet_blockage_document",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_blockage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    file_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet_blockage_document", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wallet_blockage",
                schema: "core");

            migrationBuilder.DropTable(
                name: "wallet_blockage_document",
                schema: "core");
        }
    }
}
