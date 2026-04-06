using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Kkb.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class KkbAuthToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "kkb_authorization_token",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_token = table.Column<string>(type: "text", nullable: true),
                    refresh_token = table.Column<string>(type: "text", nullable: true),
                    token_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    expires_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_success = table.Column<bool>(type: "boolean", nullable: false),
                    error = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    error_description = table.Column<string>(type: "text", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kkb_authorization_token", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kkb_authorization_token",
                schema: "core");
        }
    }
}
