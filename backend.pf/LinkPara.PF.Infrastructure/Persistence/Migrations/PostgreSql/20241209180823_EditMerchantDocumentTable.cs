using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class EditMerchantDocumentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_document_sub_merchant_sub_merchant_id",
                schema: "merchant",
                table: "document");

            migrationBuilder.DropIndex(
                name: "ix_document_sub_merchant_id",
                schema: "merchant",
                table: "document");

            migrationBuilder.DropColumn(
                name: "sub_merchant_id",
                schema: "merchant",
                table: "document");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "sub_merchant_id",
                schema: "merchant",
                table: "document",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_document_sub_merchant_id",
                schema: "merchant",
                table: "document",
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
    }
}
