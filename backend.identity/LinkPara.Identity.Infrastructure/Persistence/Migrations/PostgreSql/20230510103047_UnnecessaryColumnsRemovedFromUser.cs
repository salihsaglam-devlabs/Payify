using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Identity.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class UnnecessaryColumnsRemovedFromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "document_type",
                schema: "core",
                table: "user");

            migrationBuilder.DropColumn(
                name: "fathers_name",
                schema: "core",
                table: "user");

            migrationBuilder.DropColumn(
                name: "internal_user_code",
                schema: "core",
                table: "user");

            migrationBuilder.DropColumn(
                name: "nation_country_id",
                schema: "core",
                table: "user");

            migrationBuilder.DropColumn(
                name: "profession",
                schema: "core",
                table: "user");

            migrationBuilder.DropColumn(
                name: "serial_number",
                schema: "core",
                table: "user");

            migrationBuilder.DropColumn(
                name: "source_channel",
                schema: "core",
                table: "user");

            migrationBuilder.DropColumn(
                name: "status_change_reason",
                schema: "core",
                table: "user");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "document_type",
                schema: "core",
                table: "user",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "fathers_name",
                schema: "core",
                table: "user",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "internal_user_code",
                schema: "core",
                table: "user",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nation_country_id",
                schema: "core",
                table: "user",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profession",
                schema: "core",
                table: "user",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "serial_number",
                schema: "core",
                table: "user",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_channel",
                schema: "core",
                table: "user",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status_change_reason",
                schema: "core",
                table: "user",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);
        }
    }
}
