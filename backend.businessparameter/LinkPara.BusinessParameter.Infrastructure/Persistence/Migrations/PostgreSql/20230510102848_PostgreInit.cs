using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.BusinessParameter.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostgreInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "parameter_group",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    explanation = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    parameter_value_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parameter_group", x => x.id);
                    table.UniqueConstraint("ak_parameter_group_group_code", x => x.group_code);
                });

            migrationBuilder.CreateTable(
                name: "parameter",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    parameter_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    parameter_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parameter", x => x.id);
                    table.UniqueConstraint("ak_parameter_parameter_code_group_code", x => new { x.parameter_code, x.group_code });
                    table.ForeignKey(
                        name: "fk_parameter_parameter_group_parameter_group_id",
                        column: x => x.group_code,
                        principalSchema: "core",
                        principalTable: "parameter_group",
                        principalColumn: "group_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "parameter_template",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    template_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    data_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_length = table.Column<int>(type: "integer", nullable: false),
                    explanation = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parameter_template", x => x.id);
                    table.ForeignKey(
                        name: "fk_parameter_template_parameter_group_parameter_group_id",
                        column: x => x.group_code,
                        principalSchema: "core",
                        principalTable: "parameter_group",
                        principalColumn: "group_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "parameter_template_value",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    parameter_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    template_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    template_value = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parameter_template_value", x => x.id);
                    table.ForeignKey(
                        name: "fk_parameter_template_value_parameter_group_parameter_group_id",
                        column: x => x.group_code,
                        principalSchema: "core",
                        principalTable: "parameter_group",
                        principalColumn: "group_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_parameter_template_value_parameter_parameter_id",
                        columns: x => new { x.parameter_code, x.group_code },
                        principalSchema: "core",
                        principalTable: "parameter",
                        principalColumns: new[] { "parameter_code", "group_code" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_parameter_group_code_parameter_code",
                schema: "core",
                table: "parameter",
                columns: new[] { "group_code", "parameter_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_parameter_group_group_code",
                schema: "core",
                table: "parameter_group",
                column: "group_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_parameter_template_group_code_template_code",
                schema: "core",
                table: "parameter_template",
                columns: new[] { "group_code", "template_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_parameter_template_value_group_code_template_code_parameter",
                schema: "core",
                table: "parameter_template_value",
                columns: new[] { "group_code", "template_code", "parameter_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_parameter_template_value_parameter_code_group_code",
                schema: "core",
                table: "parameter_template_value",
                columns: new[] { "parameter_code", "group_code" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parameter_template",
                schema: "core");

            migrationBuilder.DropTable(
                name: "parameter_template_value",
                schema: "core");

            migrationBuilder.DropTable(
                name: "parameter",
                schema: "core");

            migrationBuilder.DropTable(
                name: "parameter_group",
                schema: "core");
        }
    }
}
