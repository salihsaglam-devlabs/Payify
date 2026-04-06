using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PricingProfileInstallmentUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "profile_settlement_mode",
                schema: "core",
                table: "pricing_profile",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "SingleBlock");

            migrationBuilder.AddColumn<string>(
                name: "profile_settlement_mode",
                schema: "core",
                table: "cost_profile",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "SingleBlock");

            migrationBuilder.CreateTable(
                name: "cost_profile_installment",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    installment_sequence = table.Column<int>(type: "integer", nullable: false),
                    blocked_day_number = table.Column<int>(type: "integer", nullable: false),
                    cost_profile_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cost_profile_installment", x => x.id);
                    table.ForeignKey(
                        name: "fk_cost_profile_installment_cost_profile_item_cost_profile_ite",
                        column: x => x.cost_profile_item_id,
                        principalSchema: "core",
                        principalTable: "cost_profile_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pricing_profile_installment",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    installment_sequence = table.Column<int>(type: "integer", nullable: false),
                    blocked_day_number = table.Column<int>(type: "integer", nullable: false),
                    pricing_profile_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pricing_profile_installment", x => x.id);
                    table.ForeignKey(
                        name: "fk_pricing_profile_installment_pricing_profile_item_pricing_pr",
                        column: x => x.pricing_profile_item_id,
                        principalSchema: "core",
                        principalTable: "pricing_profile_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cost_profile_installment_cost_profile_item_id_installment_s",
                schema: "core",
                table: "cost_profile_installment",
                columns: new[] { "cost_profile_item_id", "installment_sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pricing_profile_installment_pricing_profile_item_id_install",
                schema: "core",
                table: "pricing_profile_installment",
                columns: new[] { "pricing_profile_item_id", "installment_sequence" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cost_profile_installment",
                schema: "core");

            migrationBuilder.DropTable(
                name: "pricing_profile_installment",
                schema: "core");

            migrationBuilder.DropColumn(
                name: "profile_settlement_mode",
                schema: "core",
                table: "pricing_profile");

            migrationBuilder.DropColumn(
                name: "profile_settlement_mode",
                schema: "core",
                table: "cost_profile");
        }
    }
}
