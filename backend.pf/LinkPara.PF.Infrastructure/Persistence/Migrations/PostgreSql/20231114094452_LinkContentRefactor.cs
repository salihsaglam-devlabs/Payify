using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class LinkContentRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "link_content_version",
                schema: "link");

            migrationBuilder.DropTable(
                name: "link_logo",
                schema: "link");

            migrationBuilder.DropTable(
                name: "link_content",
                schema: "link");

            migrationBuilder.CreateTable(
                name: "merchant_content",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    content_source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_content", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "merchant_logo",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bytes = table.Column<byte[]>(type: "bytea", nullable: true),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_logo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "merchant_content_version",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_content_version", x => x.id);
                    table.ForeignKey(
                        name: "fk_merchant_content_version_merchant_content_merchant_content_",
                        column: x => x.merchant_content_id,
                        principalSchema: "merchant",
                        principalTable: "merchant_content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_merchant_content_record_status",
                schema: "merchant",
                table: "merchant_content",
                column: "record_status");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_content_version_merchant_content_id",
                schema: "merchant",
                table: "merchant_content_version",
                column: "merchant_content_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_logo_merchant_id",
                schema: "merchant",
                table: "merchant_logo",
                column: "merchant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "merchant_content_version",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "merchant_logo",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "merchant_content",
                schema: "merchant");

            migrationBuilder.CreateTable(
                name: "link_content",
                schema: "link",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_link_content", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "link_logo",
                schema: "link",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bytes = table.Column<byte[]>(type: "bytea", nullable: true),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_link_logo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "link_content_version",
                schema: "link",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    link_content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_link_content_version", x => x.id);
                    table.ForeignKey(
                        name: "fk_link_content_version_link_content_link_content_id",
                        column: x => x.link_content_id,
                        principalSchema: "link",
                        principalTable: "link_content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_link_content_record_status",
                schema: "link",
                table: "link_content",
                column: "record_status");

            migrationBuilder.CreateIndex(
                name: "ix_link_content_version_link_content_id",
                schema: "link",
                table: "link_content_version",
                column: "link_content_id");

            migrationBuilder.CreateIndex(
                name: "ix_link_logo_merchant_id",
                schema: "link",
                table: "link_logo",
                column: "merchant_id");
        }
    }
}
