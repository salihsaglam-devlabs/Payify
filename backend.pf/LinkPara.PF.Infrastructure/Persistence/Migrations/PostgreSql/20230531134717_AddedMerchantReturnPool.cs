using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedMerchantReturnPool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "return_status",
                schema: "merchant",
                table: "transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "return_approval_status",
                schema: "merchant",
                table: "merchant",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "merchant_return_pool",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    action_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    action_user = table.Column<Guid>(type: "uuid", nullable: false),
                    return_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    order_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    conversation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    client_ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    language_code = table.Column<string>(type: "text", nullable: true),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_return_pool", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "merchant_return_pool",
                schema: "merchant");

            migrationBuilder.DropColumn(
                name: "return_status",
                schema: "merchant",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "return_approval_status",
                schema: "merchant",
                table: "merchant");
        }
    }
}
