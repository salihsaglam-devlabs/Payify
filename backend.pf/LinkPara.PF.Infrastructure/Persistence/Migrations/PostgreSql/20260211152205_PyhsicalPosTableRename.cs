using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PyhsicalPosTableRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "merchant_pyhsical_pos",
                schema: "merchant");

            migrationBuilder.CreateTable(
                name: "merchant_physical_pos",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_physical_device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    physical_pos_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pos_merchant_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pos_terminal_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_physical_pos", x => x.id);
                    table.ForeignKey(
                        name: "fk_merchant_physical_pos_merchant_physical_device_merchant_phy",
                        column: x => x.merchant_physical_device_id,
                        principalSchema: "merchant",
                        principalTable: "merchant_physical_device",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_merchant_physical_pos_physical_pos_physical_pos_id",
                        column: x => x.physical_pos_id,
                        principalSchema: "physical",
                        principalTable: "physical_pos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_merchant_physical_pos_merchant_physical_device_id",
                schema: "merchant",
                table: "merchant_physical_pos",
                column: "merchant_physical_device_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_physical_pos_physical_pos_id",
                schema: "merchant",
                table: "merchant_physical_pos",
                column: "physical_pos_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "merchant_physical_pos",
                schema: "merchant");

            migrationBuilder.CreateTable(
                name: "merchant_pyhsical_pos",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_physical_device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    physical_pos_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pos_merchant_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pos_terminal_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_pyhsical_pos", x => x.id);
                    table.ForeignKey(
                        name: "fk_merchant_pyhsical_pos_merchant_physical_device_merchant_phy",
                        column: x => x.merchant_physical_device_id,
                        principalSchema: "merchant",
                        principalTable: "merchant_physical_device",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_merchant_pyhsical_pos_physical_pos_physical_pos_id",
                        column: x => x.physical_pos_id,
                        principalSchema: "physical",
                        principalTable: "physical_pos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_merchant_pyhsical_pos_merchant_physical_device_id",
                schema: "merchant",
                table: "merchant_pyhsical_pos",
                column: "merchant_physical_device_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_pyhsical_pos_physical_pos_id",
                schema: "merchant",
                table: "merchant_pyhsical_pos",
                column: "physical_pos_id");
        }
    }
}
