using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class CommercialPricingTablesCreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_commercial",
                schema: "core",
                table: "account",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "account_activity",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transfer_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    sender = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    transaction_direction = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    receiver = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    own_account = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_activity", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pricing_commercial",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    max_distinct_sender_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_distinct_sender_count_with_amount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_distinct_sender_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    pricing_commercial_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activation_date = table.Column<DateTime>(type: "date", nullable: false),
                    commission_rate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    pricing_commercial_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pricing_commercial", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_activity_account_id_year_month",
                schema: "core",
                table: "account_activity",
                columns: new[] { "account_id", "year", "month" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_activity",
                schema: "core");

            migrationBuilder.DropTable(
                name: "pricing_commercial",
                schema: "core");

            migrationBuilder.DropColumn(
                name: "error_message",
                schema: "core",
                table: "transfer_order");

            migrationBuilder.DropColumn(
                name: "is_commercial",
                schema: "core",
                table: "account");
        }
    }
}
