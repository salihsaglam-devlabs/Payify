using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class CustomerRevert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customer",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    company_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    commercial_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    trade_registration_number = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    tax_administration = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tax_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    mersis_number = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    country = table.Column<int>(type: "integer", nullable: false),
                    country_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city = table.Column<int>(type: "integer", nullable: false),
                    city_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    district = table.Column<int>(type: "integer", nullable: false),
                    district_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    contact_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_contact_person_contact_person_id",
                        column: x => x.contact_person_id,
                        principalSchema: "core",
                        principalTable: "contact_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_merchant_customer_id",
                schema: "merchant",
                table: "merchant",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_contact_person_id",
                schema: "core",
                table: "customer",
                column: "contact_person_id");

            migrationBuilder.AddForeignKey(
                name: "fk_merchant_customer_customer_id",
                schema: "merchant",
                table: "merchant",
                column: "customer_id",
                principalSchema: "core",
                principalTable: "customer",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_merchant_customer_customer_id",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropTable(
                name: "customer",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "ix_merchant_customer_id",
                schema: "merchant",
                table: "merchant");
        }
    }
}
