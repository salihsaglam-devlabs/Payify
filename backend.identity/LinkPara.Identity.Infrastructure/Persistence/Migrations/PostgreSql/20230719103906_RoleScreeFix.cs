using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Identity.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class RoleScreeFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_hide_sensitive_data",
                schema: "core",
                table: "role",
                newName: "does_hide_sensitive_data");

            migrationBuilder.CreateTable(
                name: "screen",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    module = table.Column<string>(type: "text", nullable: true),
                    operation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_screen", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_screen",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    screen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_screen", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_screen_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "core",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_screen_screen_screen_id",
                        column: x => x.screen_id,
                        principalTable: "screen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "screen_claim",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_screen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_value = table.Column<string>(type: "text", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_screen_claim", x => x.id);
                    table.ForeignKey(
                        name: "fk_screen_claim_role_screen_role_screen_id",
                        column: x => x.role_screen_id,
                        principalTable: "role_screen",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_role_screen_role_id",
                table: "role_screen",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_screen_screen_id",
                table: "role_screen",
                column: "screen_id");

            migrationBuilder.CreateIndex(
                name: "ix_screen_claim_role_screen_id",
                table: "screen_claim",
                column: "role_screen_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "screen_claim");

            migrationBuilder.DropTable(
                name: "role_screen");

            migrationBuilder.DropTable(
                name: "screen");

            migrationBuilder.RenameColumn(
                name: "does_hide_sensitive_data",
                schema: "core",
                table: "role",
                newName: "is_hide_sensitive_data");
        }
    }
}
