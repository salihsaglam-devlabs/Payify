using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedPhysicalPosEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "physical");

            migrationBuilder.AlterColumn<Guid>(
                name: "vpos_id",
                schema: "core",
                table: "cost_profile",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "physical_pos_id",
                schema: "core",
                table: "cost_profile",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pos_type",
                schema: "core",
                table: "cost_profile",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Virtual");

            migrationBuilder.CreateTable(
                name: "device_inventory",
                schema: "physical",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    serial_no = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    contactless_separator = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    physical_pos_vendor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    device_model = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    device_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    device_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    inventory_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_inventory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "physical_pos",
                schema: "physical",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    vpos_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    acquire_bank_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vpos_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    pf_main_merchant_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_physical_pos", x => x.id);
                    table.ForeignKey(
                        name: "fk_physical_pos_acquire_bank_acquire_bank_id",
                        column: x => x.acquire_bank_id,
                        principalSchema: "bank",
                        principalTable: "acquire_bank",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "merchant_physical_device",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_psp_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_pin_pad = table.Column<bool>(type: "boolean", nullable: false),
                    connection_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    assignment_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fiscal_no = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_inventory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_physical_device", x => x.id);
                    table.ForeignKey(
                        name: "fk_merchant_physical_device_device_inventory_device_inventory_",
                        column: x => x.device_inventory_id,
                        principalSchema: "physical",
                        principalTable: "device_inventory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_merchant_physical_device_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "physical_pos_currency",
                schema: "physical",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    physical_pos_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_physical_pos_currency", x => x.id);
                    table.ForeignKey(
                        name: "fk_physical_pos_currency_currency_currency_code",
                        column: x => x.currency_code,
                        principalSchema: "core",
                        principalTable: "currency",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_physical_pos_currency_physical_pos_physical_pos_id",
                        column: x => x.physical_pos_id,
                        principalSchema: "physical",
                        principalTable: "physical_pos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "merchant_device_api_key",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    public_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    private_key_encrypted = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    merchant_physical_device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_device_api_key", x => x.id);
                    table.ForeignKey(
                        name: "fk_merchant_device_api_key_merchant_physical_device_merchant_p",
                        column: x => x.merchant_physical_device_id,
                        principalSchema: "merchant",
                        principalTable: "merchant_physical_device",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "merchant_pyhsical_pos",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_physical_device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    physical_pos_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
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
                name: "ix_cost_profile_physical_pos_id",
                schema: "core",
                table: "cost_profile",
                column: "physical_pos_id");

            migrationBuilder.CreateIndex(
                name: "ix_device_inventory_device_model_physical_pos_vendor_device_ty",
                schema: "physical",
                table: "device_inventory",
                columns: new[] { "device_model", "physical_pos_vendor", "device_type", "serial_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_merchant_device_api_key_merchant_physical_device_id",
                schema: "merchant",
                table: "merchant_device_api_key",
                column: "merchant_physical_device_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_physical_device_device_inventory_id",
                schema: "merchant",
                table: "merchant_physical_device",
                column: "device_inventory_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_physical_device_merchant_id",
                schema: "merchant",
                table: "merchant_physical_device",
                column: "merchant_id");

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

            migrationBuilder.CreateIndex(
                name: "ix_physical_pos_acquire_bank_id",
                schema: "physical",
                table: "physical_pos",
                column: "acquire_bank_id");

            migrationBuilder.CreateIndex(
                name: "ix_physical_pos_currency_currency_code",
                schema: "physical",
                table: "physical_pos_currency",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "ix_physical_pos_currency_physical_pos_id",
                schema: "physical",
                table: "physical_pos_currency",
                column: "physical_pos_id");

            migrationBuilder.AddForeignKey(
                name: "fk_cost_profile_physical_pos_physical_pos_id",
                schema: "core",
                table: "cost_profile",
                column: "physical_pos_id",
                principalSchema: "physical",
                principalTable: "physical_pos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cost_profile_physical_pos_physical_pos_id",
                schema: "core",
                table: "cost_profile");

            migrationBuilder.DropTable(
                name: "merchant_device_api_key",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "merchant_pyhsical_pos",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "physical_pos_currency",
                schema: "physical");

            migrationBuilder.DropTable(
                name: "merchant_physical_device",
                schema: "merchant");

            migrationBuilder.DropTable(
                name: "physical_pos",
                schema: "physical");

            migrationBuilder.DropTable(
                name: "device_inventory",
                schema: "physical");

            migrationBuilder.DropIndex(
                name: "ix_cost_profile_physical_pos_id",
                schema: "core",
                table: "cost_profile");

            migrationBuilder.DropColumn(
                name: "physical_pos_id",
                schema: "core",
                table: "cost_profile");

            migrationBuilder.DropColumn(
                name: "pos_type",
                schema: "core",
                table: "cost_profile");

            migrationBuilder.AlterColumn<Guid>(
                name: "vpos_id",
                schema: "core",
                table: "cost_profile",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
