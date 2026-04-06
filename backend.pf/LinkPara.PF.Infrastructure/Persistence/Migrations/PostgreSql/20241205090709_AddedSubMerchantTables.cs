using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedSubMerchantTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "submerchant");

            migrationBuilder.AddColumn<string>(
                name: "merchant_type",
                schema: "merchant",
                table: "merchant",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "sub_merchant_id",
                schema: "merchant",
                table: "document",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "sub_merchant",
                schema: "submerchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    merchant_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    city = table.Column<int>(type: "integer", nullable: false),
                    city_name = table.Column<string>(type: "text", nullable: true),
                    is_manuel_payment3d_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_link_payment3d_required = table.Column<bool>(type: "boolean", nullable: false),
                    pre_authorization_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    payment_reverse_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    payment_return_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    installment_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    is3d_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_excess_return_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    international_card_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sub_merchant", x => x.id);
                    table.ForeignKey(
                        name: "fk_sub_merchant_merchant_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchant",
                        principalTable: "merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sub_merchant_document",
                schema: "submerchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    sub_merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sub_merchant_document", x => x.id);
                    table.ForeignKey(
                        name: "fk_sub_merchant_document_sub_merchant_sub_merchant_id",
                        column: x => x.sub_merchant_id,
                        principalSchema: "submerchant",
                        principalTable: "sub_merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sub_merchant_limit",
                schema: "submerchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_limit_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    period = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    limit_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    max_piece = table.Column<int>(type: "integer", nullable: true),
                    max_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    sub_merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sub_merchant_limit", x => x.id);
                    table.ForeignKey(
                        name: "fk_sub_merchant_limit_sub_merchant_sub_merchant_id",
                        column: x => x.sub_merchant_id,
                        principalSchema: "submerchant",
                        principalTable: "sub_merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_document_sub_merchant_id",
                schema: "merchant",
                table: "document",
                column: "sub_merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_sub_merchant_merchant_id",
                schema: "submerchant",
                table: "sub_merchant",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_sub_merchant_document_sub_merchant_id",
                schema: "submerchant",
                table: "sub_merchant_document",
                column: "sub_merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_sub_merchant_limit_sub_merchant_id",
                schema: "submerchant",
                table: "sub_merchant_limit",
                column: "sub_merchant_id");

            migrationBuilder.AddForeignKey(
                name: "fk_document_sub_merchant_sub_merchant_id",
                schema: "merchant",
                table: "document",
                column: "sub_merchant_id",
                principalSchema: "submerchant",
                principalTable: "sub_merchant",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_document_sub_merchant_sub_merchant_id",
                schema: "merchant",
                table: "document");

            migrationBuilder.DropTable(
                name: "sub_merchant_document",
                schema: "submerchant");

            migrationBuilder.DropTable(
                name: "sub_merchant_limit",
                schema: "submerchant");

            migrationBuilder.DropTable(
                name: "sub_merchant",
                schema: "submerchant");

            migrationBuilder.DropIndex(
                name: "ix_document_sub_merchant_id",
                schema: "merchant",
                table: "document");

            migrationBuilder.DropColumn(
                name: "merchant_type",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "sub_merchant_id",
                schema: "merchant",
                table: "document");
        }
    }
}
