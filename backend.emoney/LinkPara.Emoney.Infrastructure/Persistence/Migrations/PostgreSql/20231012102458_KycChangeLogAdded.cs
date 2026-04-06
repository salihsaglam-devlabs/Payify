using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class KycChangeLogAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_kyc_change",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    validation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    old_kyc_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    new_kyc_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_upgraded = table.Column<bool>(type: "boolean", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_kyc_change", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_kyc_change",
                schema: "core");
        }
    }
}
