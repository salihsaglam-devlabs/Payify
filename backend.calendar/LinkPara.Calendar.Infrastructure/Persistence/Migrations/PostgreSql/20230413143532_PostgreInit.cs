using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Calendar.Infrastructure.Persistence.Migrations
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
                name: "holiday",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    country_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    holiday_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_holiday", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "holiday_detail",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    duration_in_days = table.Column<int>(type: "integer", nullable: false),
                    date_of_holiday = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    beginning_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ending_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    holiday_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_holiday_detail", x => x.id);
                    table.ForeignKey(
                        name: "fk_holiday_detail_holiday_holiday_id",
                        column: x => x.holiday_id,
                        principalSchema: "core",
                        principalTable: "holiday",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_holiday_country_code",
                schema: "core",
                table: "holiday",
                column: "country_code");

            migrationBuilder.CreateIndex(
                name: "ix_holiday_detail_holiday_id",
                schema: "core",
                table: "holiday_detail",
                column: "holiday_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "holiday_detail",
                schema: "core");

            migrationBuilder.DropTable(
                name: "holiday",
                schema: "core");
        }
    }
}
