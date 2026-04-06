using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MerchantStatementMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) 
        {
            migrationBuilder.CreateTable(
                name: "merchant_statement",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_statement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mail_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    statement_start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    statement_end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    file_path = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    file_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    request = table.Column<string>(type: "text", nullable: true),
                    response = table.Column<string>(type: "text", nullable: true),
                    error_code = table.Column<string>(type: "text", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                }, 
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_statement", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "merchant_statement",
                schema: "merchant");
        }
    }
}
