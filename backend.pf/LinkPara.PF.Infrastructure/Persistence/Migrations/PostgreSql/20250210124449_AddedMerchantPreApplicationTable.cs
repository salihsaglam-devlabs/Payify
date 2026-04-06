using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedMerchantPreApplicationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "merchantPreApplication");

            migrationBuilder.CreateTable(
                name: "merchant_pre_application",
                schema: "merchantPreApplication",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    product_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    monthly_turnover = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    application_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    website = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    consent_confirmation = table.Column<bool>(type: "boolean", nullable: false),
                    kvkk_confirmation = table.Column<bool>(type: "boolean", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_pre_application", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "merchant_pre_application_history",
                schema: "merchantPreApplication",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pending_pos_application_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_pre_application_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    operation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    operation_date = table.Column<DateTime>(type: "date", nullable: false),
                    operation_note = table.Column<string>(type: "text", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_pre_application_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_merchant_pre_application_history_merchant_pre_application_m",
                        column: x => x.merchant_pre_application_id,
                        principalSchema: "merchantPreApplication",
                        principalTable: "merchant_pre_application",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_merchant_pre_application_history_merchant_pre_application_id",
                schema: "merchantPreApplication",
                table: "merchant_pre_application_history",
                column: "merchant_pre_application_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "merchant_pre_application_history",
                schema: "merchantPreApplication");

            migrationBuilder.DropTable(
                name: "merchant_pre_application",
                schema: "merchantPreApplication");
        }
    }
}
