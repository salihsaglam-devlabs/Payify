using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ManualTransferReferenceTableAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "manual_transfer_reference",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    approval_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_manual_transfer_reference", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_manual_transfer_reference_transaction_id",
                schema: "core",
                table: "manual_transfer_reference",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_manual_transfer_reference_transaction_id_document_type",
                schema: "core",
                table: "manual_transfer_reference",
                columns: new[] { "transaction_id", "document_type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "manual_transfer_reference",
                schema: "core");
        }
    }
}
