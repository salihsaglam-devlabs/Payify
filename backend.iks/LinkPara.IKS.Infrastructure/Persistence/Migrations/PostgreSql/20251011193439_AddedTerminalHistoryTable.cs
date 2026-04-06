using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.IKS.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedTerminalHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "created_name_by",
                schema: "core",
                table: "iks_terminal",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "terminal_status",
                schema: "core",
                table: "iks_terminal",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "vpos_id",
                schema: "core",
                table: "iks_terminal",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "iks_terminal_history",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vpos_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    new_data = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    old_data = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    changed_field = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    response_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    response_code_explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    query_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    terminal_record_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iks_terminal_history", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "iks_terminal_history",
                schema: "core");

            migrationBuilder.DropColumn(
                name: "created_name_by",
                schema: "core",
                table: "iks_terminal");

            migrationBuilder.DropColumn(
                name: "terminal_status",
                schema: "core",
                table: "iks_terminal");

            migrationBuilder.DropColumn(
                name: "vpos_id",
                schema: "core",
                table: "iks_terminal");
        }
    }
}
