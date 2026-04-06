using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class CompanyPoolAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "company_pool",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_pool_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    company_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    phone_code = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    land_phone_code = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    land_phone_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    web_site_url = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    tax_administration = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tax_number = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    trade_registration_number = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    iban = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    country = table.Column<int>(type: "integer", nullable: false),
                    country_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city = table.Column<int>(type: "integer", nullable: false),
                    city_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    district = table.Column<int>(type: "integer", nullable: false),
                    district_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    authorized_person_identity_number = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    authorized_person_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    authorized_person_surname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    authorized_person_birth_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    authorized_person_company_phone_code = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    authorized_person_company_phone_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    reject_reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    action_user = table.Column<Guid>(type: "uuid", nullable: false),
                    action_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_pool", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_pool",
                schema: "core");
        }
    }
}
