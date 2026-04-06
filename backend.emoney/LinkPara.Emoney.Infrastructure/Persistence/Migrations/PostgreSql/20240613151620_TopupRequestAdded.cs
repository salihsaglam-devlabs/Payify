using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class TopupRequestAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "card_topup_request_id",
                schema: "core",
                table: "transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "card_topup_request",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: true),
                    card_brand = table.Column<string>(type: "text", nullable: true),
                    commission_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    card_number = table.Column<string>(type: "text", nullable: true),
                    card_token = table.Column<string>(type: "text", nullable: true),
                    threed_session_id = table.Column<string>(type: "text", nullable: true),
                    provision_number = table.Column<string>(type: "text", nullable: true),
                    order_number = table.Column<string>(type: "text", nullable: true),
                    order_id = table.Column<string>(type: "text", nullable: true),
                    conversation_id = table.Column<string>(type: "text", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    error_code = table.Column<string>(type: "text", nullable: true),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_number = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_card_topup_request", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "card_topup_request",
                schema: "core");

            migrationBuilder.DropColumn(
                name: "card_topup_request_id",
                schema: "core",
                table: "transaction");
        }
    }
}
