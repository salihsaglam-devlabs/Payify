using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.IKS.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class VPS_TerminalCreateMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "iks_terminal",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    global_merchant_id = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    psp_merchant_id = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    terminal_id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    status_code = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    type = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    brand_code = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    model = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    serial_no = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    owner_psp_no = table.Column<int>(type: "integer", nullable: false),
                    owner_terminal_id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    brand_sharing = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    pin_pad = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    contactless = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    connection_type = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    virtual_pos_url = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    hosting_tax_no = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    hosting_trade_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    hosting_url = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    payment_gw_tax_no = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    payment_gw_trade_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    payment_gw_url = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    service_provider_psp_no = table.Column<int>(type: "integer", nullable: false),
                    fiscal_no = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    tech_pos = table.Column<int>(type: "integer", nullable: false),
                    service_provider_psp_merchant_id = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    pf_main_merchant_id = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    response_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    response_code_explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iks_terminal", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "iks_terminal",
                schema: "core");
        }
    }
}
