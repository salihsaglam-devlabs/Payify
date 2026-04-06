using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class LinkUrlGenerate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "link");

            migrationBuilder.CreateTable(
                name: "link",
                schema: "link",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    link_url = table.Column<string>(type: "text", nullable: true),
                    link_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    link_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    current_usage_count = table.Column<int>(type: "integer", nullable: false),
                    max_usage_count = table.Column<int>(type: "integer", nullable: false),
                    order_id = table.Column<string>(type: "text", nullable: true),
                    link_amount_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    commission_from_customer = table.Column<bool>(type: "boolean", nullable: false),
                    is3d_required = table.Column<bool>(type: "boolean", nullable: false),
                    merchant_name = table.Column<string>(type: "text", nullable: true),
                    merchant_number = table.Column<string>(type: "text", nullable: true),
                    product_name = table.Column<string>(type: "text", nullable: true),
                    product_description = table.Column<string>(type: "text", nullable: true),
                    return_url = table.Column<string>(type: "text", nullable: true),
                    is_name_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_email_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_phone_number_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_address_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_note_required = table.Column<bool>(type: "boolean", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_link", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "link_installment",
                schema: "link",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    link_id = table.Column<Guid>(type: "uuid", nullable: false),
                    installment = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_link_installment", x => x.id);
                    table.ForeignKey(
                        name: "fk_link_installment_link_link_id",
                        column: x => x.link_id,
                        principalSchema: "link",
                        principalTable: "link",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_link_installment_link_id",
                schema: "link",
                table: "link_installment",
                column: "link_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "link_installment",
                schema: "link");

            migrationBuilder.DropTable(
                name: "link",
                schema: "link");
        }
    }
}
