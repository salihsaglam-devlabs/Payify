using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedSubMerchantFieldsForReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "sub_merchant_id",
                schema: "merchant",
                table: "transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sub_merchant_name",
                schema: "merchant",
                table: "transaction",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sub_merchant_number",
                schema: "merchant",
                table: "transaction",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "sub_merchant_id",
                schema: "link",
                table: "link",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sub_merchant_name",
                schema: "link",
                table: "link",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "sub_merchant_number",
                schema: "link",
                table: "link",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "daily_usage",
                schema: "submerchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    sub_merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_limit_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_usage", x => x.id);
                    table.ForeignKey(
                        name: "fk_daily_usage_sub_merchant_sub_merchant_id",
                        column: x => x.sub_merchant_id,
                        principalSchema: "submerchant",
                        principalTable: "sub_merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "monthly_usage",
                schema: "submerchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    sub_merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_limit_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_monthly_usage", x => x.id);
                    table.ForeignKey(
                        name: "fk_monthly_usage_sub_merchant_sub_merchant_id",
                        column: x => x.sub_merchant_id,
                        principalSchema: "submerchant",
                        principalTable: "sub_merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_daily_usage_sub_merchant_id",
                schema: "submerchant",
                table: "daily_usage",
                column: "sub_merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_monthly_usage_sub_merchant_id",
                schema: "submerchant",
                table: "monthly_usage",
                column: "sub_merchant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_usage",
                schema: "submerchant");

            migrationBuilder.DropTable(
                name: "monthly_usage",
                schema: "submerchant");

            migrationBuilder.DropColumn(
                name: "sub_merchant_id",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "sub_merchant_name",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "sub_merchant_number",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "sub_merchant_id",
                schema: "link",
                table: "link");

            migrationBuilder.DropColumn(
                name: "sub_merchant_name",
                schema: "link",
                table: "link");

            migrationBuilder.DropColumn(
                name: "sub_merchant_number",
                schema: "link",
                table: "link");
        }
    }
}
