using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.CampaignManagement.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostgreInitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "authorization_token",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    refresh_token_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_authorization_token", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "i_wallet_card",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    card_application_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    full_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    identity_number = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    email = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    address_detail = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                    city_id = table.Column<int>(type: "integer", nullable: false),
                    town_id = table.Column<int>(type: "integer", nullable: false),
                    user_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_approved_individual_framework_agreement = table.Column<bool>(type: "boolean", nullable: false),
                    individual_framework_agreement_version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_approved_preliminary_information_agreement = table.Column<bool>(type: "boolean", nullable: false),
                    preliminary_information_agreement_version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_approved_kvkk_agreement = table.Column<bool>(type: "boolean", nullable: false),
                    kvkk_agreement_version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_approved_commercial_electronic_communication_aggrement = table.Column<bool>(type: "boolean", nullable: false),
                    commercial_electronic_communication_aggrement_version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    card_id = table.Column<int>(type: "integer", nullable: false),
                    card_number = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    customer_branch_id = table.Column<int>(type: "integer", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "character varying(800)", maxLength: 800, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_i_wallet_card", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "i_wallet_qr_code",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    card_id = table.Column<int>(type: "integer", nullable: false),
                    qr_code = table.Column<int>(type: "integer", nullable: false),
                    expires_in = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    message = table.Column<string>(type: "character varying(800)", maxLength: 800, nullable: true),
                    i_wallet_qr_code_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_i_wallet_qr_code", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_authorization_token_expiry_date",
                schema: "core",
                table: "authorization_token",
                column: "expiry_date");

            migrationBuilder.CreateIndex(
                name: "ix_authorization_token_refresh_token_date",
                schema: "core",
                table: "authorization_token",
                column: "refresh_token_date");

            migrationBuilder.CreateIndex(
                name: "ix_i_wallet_card_user_id_wallet_number",
                schema: "core",
                table: "i_wallet_card",
                columns: new[] { "user_id", "wallet_number" });

            migrationBuilder.CreateIndex(
                name: "ix_i_wallet_qr_code_qr_code",
                schema: "core",
                table: "i_wallet_qr_code",
                column: "qr_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "authorization_token",
                schema: "core");

            migrationBuilder.DropTable(
                name: "i_wallet_card",
                schema: "core");

            migrationBuilder.DropTable(
                name: "i_wallet_qr_code",
                schema: "core");
        }
    }
}
