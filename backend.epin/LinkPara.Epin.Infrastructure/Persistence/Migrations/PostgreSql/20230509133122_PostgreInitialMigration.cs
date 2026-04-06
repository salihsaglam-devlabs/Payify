using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Epin.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostgreInitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "order_history",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<int>(type: "integer", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    pin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    external_product_id = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    vat = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "publisher",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_publisher", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_summary",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reconciliation_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    external_total = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    order_total = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    external_count = table.Column<int>(type: "integer", nullable: false),
                    order_count = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    reconciliation_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    organization = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reconciliation_summary", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "brand",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    type = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    image = table.Column<string>(type: "character varying(4001)", maxLength: 4001, nullable: false),
                    summary = table.Column<string>(type: "character varying(4001)", maxLength: 4001, nullable: false),
                    description = table.Column<string>(type: "character varying(4001)", maxLength: 4001, nullable: false),
                    publisher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_brand", x => x.id);
                    table.ForeignKey(
                        name: "fk_brand_publisher_publisher_id",
                        column: x => x.publisher_id,
                        principalSchema: "core",
                        principalTable: "publisher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<int>(type: "integer", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    pin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_product_id = table.Column<int>(type: "integer", nullable: false),
                    provision_reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    wallet_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    publisher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    brand_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    equivalent = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    user_full_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    error_message = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    transaction_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_brand_brand_id",
                        column: x => x.brand_id,
                        principalSchema: "core",
                        principalTable: "brand",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_publisher_publisher_id",
                        column: x => x.publisher_id,
                        principalSchema: "core",
                        principalTable: "publisher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    equivalent = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    vat = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    publisher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    brand_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_brand_brand_id",
                        column: x => x.brand_id,
                        principalSchema: "core",
                        principalTable: "brand",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_publisher_publisher_id",
                        column: x => x.publisher_id,
                        principalSchema: "core",
                        principalTable: "publisher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_detail",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reconciliation_summary_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reconciliation_detail_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    external_order_id = table.Column<int>(type: "integer", nullable: false),
                    external_total = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    has_internal_orders = table.Column<bool>(type: "boolean", nullable: false),
                    has_external_orders = table.Column<bool>(type: "boolean", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    internal_order_error_message = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order_history_id = table.Column<Guid>(type: "uuid", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reconciliation_detail", x => x.id);
                    table.ForeignKey(
                        name: "fk_reconciliation_detail_order_history_order_history_id",
                        column: x => x.order_history_id,
                        principalSchema: "core",
                        principalTable: "order_history",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reconciliation_detail_order_order_id",
                        column: x => x.order_id,
                        principalSchema: "core",
                        principalTable: "order",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reconciliation_detail_reconciliation_summary_reconciliation",
                        column: x => x.reconciliation_summary_id,
                        principalSchema: "core",
                        principalTable: "reconciliation_summary",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_brand_publisher_id",
                schema: "core",
                table: "brand",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_brand_id",
                schema: "core",
                table: "order",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_publisher_id",
                schema: "core",
                table: "order",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_reference_id",
                schema: "core",
                table: "order",
                column: "reference_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_brand_id",
                schema: "core",
                table: "product",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_publisher_id",
                schema: "core",
                table: "product",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_detail_order_history_id",
                schema: "core",
                table: "reconciliation_detail",
                column: "order_history_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_detail_order_id",
                schema: "core",
                table: "reconciliation_detail",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_detail_reconciliation_summary_id",
                schema: "core",
                table: "reconciliation_detail",
                column: "reconciliation_summary_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product",
                schema: "core");

            migrationBuilder.DropTable(
                name: "reconciliation_detail",
                schema: "core");

            migrationBuilder.DropTable(
                name: "order_history",
                schema: "core");

            migrationBuilder.DropTable(
                name: "order",
                schema: "core");

            migrationBuilder.DropTable(
                name: "reconciliation_summary",
                schema: "core");

            migrationBuilder.DropTable(
                name: "brand",
                schema: "core");

            migrationBuilder.DropTable(
                name: "publisher",
                schema: "core");
        }
    }
}
