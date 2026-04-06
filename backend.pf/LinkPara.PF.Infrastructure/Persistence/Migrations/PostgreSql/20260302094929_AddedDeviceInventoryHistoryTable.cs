using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedDeviceInventoryHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "device_inventory_history",
                schema: "physical",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_inventory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_history_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    new_data = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    old_data = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    detail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_name_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_inventory_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_device_inventory_history_device_inventory_device_inventory_",
                        column: x => x.device_inventory_id,
                        principalSchema: "physical",
                        principalTable: "device_inventory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_device_inventory_history_device_inventory_id",
                schema: "physical",
                table: "device_inventory_history",
                column: "device_inventory_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_inventory_history",
                schema: "physical");
        }
    }
}
