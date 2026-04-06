using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class EditSubMerchantTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sub_merchant_document_sub_merchant_sub_merchant_id",
                schema: "submerchant",
                table: "sub_merchant_document");

            migrationBuilder.DropForeignKey(
                name: "fk_sub_merchant_limit_sub_merchant_sub_merchant_id",
                schema: "submerchant",
                table: "sub_merchant_limit");

            migrationBuilder.DropForeignKey(
                name: "fk_sub_merchant_user_sub_merchant_sub_merchant_id",
                schema: "submerchant",
                table: "sub_merchant_user");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sub_merchant_user",
                schema: "submerchant",
                table: "sub_merchant_user");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sub_merchant_limit",
                schema: "submerchant",
                table: "sub_merchant_limit");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sub_merchant_document",
                schema: "submerchant",
                table: "sub_merchant_document");

            migrationBuilder.RenameTable(
                name: "sub_merchant_user",
                schema: "submerchant",
                newName: "user",
                newSchema: "submerchant");

            migrationBuilder.RenameTable(
                name: "sub_merchant_limit",
                schema: "submerchant",
                newName: "limit",
                newSchema: "submerchant");

            migrationBuilder.RenameTable(
                name: "sub_merchant_document",
                schema: "submerchant",
                newName: "document",
                newSchema: "submerchant");

            migrationBuilder.RenameColumn(
                name: "is_manuel_payment3d_required",
                schema: "submerchant",
                table: "sub_merchant",
                newName: "is_manuel_payment_page_allowed");

            migrationBuilder.RenameColumn(
                name: "is_link_payment3d_required",
                schema: "submerchant",
                table: "sub_merchant",
                newName: "is_link_payment_page_allowed");

            migrationBuilder.RenameIndex(
                name: "ix_sub_merchant_user_sub_merchant_id",
                schema: "submerchant",
                table: "user",
                newName: "ix_user_sub_merchant_id");

            migrationBuilder.RenameIndex(
                name: "ix_sub_merchant_limit_sub_merchant_id",
                schema: "submerchant",
                table: "limit",
                newName: "ix_limit_sub_merchant_id");

            migrationBuilder.RenameIndex(
                name: "ix_sub_merchant_document_sub_merchant_id",
                schema: "submerchant",
                table: "document",
                newName: "ix_document_sub_merchant_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user",
                schema: "submerchant",
                table: "user",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_limit",
                schema: "submerchant",
                table: "limit",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_document",
                schema: "submerchant",
                table: "document",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_document_sub_merchant_sub_merchant_id",
                schema: "submerchant",
                table: "document",
                column: "sub_merchant_id",
                principalSchema: "submerchant",
                principalTable: "sub_merchant",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_limit_sub_merchant_sub_merchant_id",
                schema: "submerchant",
                table: "limit",
                column: "sub_merchant_id",
                principalSchema: "submerchant",
                principalTable: "sub_merchant",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_sub_merchant_sub_merchant_id",
                schema: "submerchant",
                table: "user",
                column: "sub_merchant_id",
                principalSchema: "submerchant",
                principalTable: "sub_merchant",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_document_sub_merchant_sub_merchant_id",
                schema: "submerchant",
                table: "document");

            migrationBuilder.DropForeignKey(
                name: "fk_limit_sub_merchant_sub_merchant_id",
                schema: "submerchant",
                table: "limit");

            migrationBuilder.DropForeignKey(
                name: "fk_user_sub_merchant_sub_merchant_id",
                schema: "submerchant",
                table: "user");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user",
                schema: "submerchant",
                table: "user");

            migrationBuilder.DropPrimaryKey(
                name: "pk_limit",
                schema: "submerchant",
                table: "limit");

            migrationBuilder.DropPrimaryKey(
                name: "pk_document",
                schema: "submerchant",
                table: "document");

            migrationBuilder.RenameTable(
                name: "user",
                schema: "submerchant",
                newName: "sub_merchant_user",
                newSchema: "submerchant");

            migrationBuilder.RenameTable(
                name: "limit",
                schema: "submerchant",
                newName: "sub_merchant_limit",
                newSchema: "submerchant");

            migrationBuilder.RenameTable(
                name: "document",
                schema: "submerchant",
                newName: "sub_merchant_document",
                newSchema: "submerchant");

            migrationBuilder.RenameColumn(
                name: "is_manuel_payment_page_allowed",
                schema: "submerchant",
                table: "sub_merchant",
                newName: "is_manuel_payment3d_required");

            migrationBuilder.RenameColumn(
                name: "is_link_payment_page_allowed",
                schema: "submerchant",
                table: "sub_merchant",
                newName: "is_link_payment3d_required");

            migrationBuilder.RenameIndex(
                name: "ix_user_sub_merchant_id",
                schema: "submerchant",
                table: "sub_merchant_user",
                newName: "ix_sub_merchant_user_sub_merchant_id");

            migrationBuilder.RenameIndex(
                name: "ix_limit_sub_merchant_id",
                schema: "submerchant",
                table: "sub_merchant_limit",
                newName: "ix_sub_merchant_limit_sub_merchant_id");

            migrationBuilder.RenameIndex(
                name: "ix_document_sub_merchant_id",
                schema: "submerchant",
                table: "sub_merchant_document",
                newName: "ix_sub_merchant_document_sub_merchant_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sub_merchant_user",
                schema: "submerchant",
                table: "sub_merchant_user",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sub_merchant_limit",
                schema: "submerchant",
                table: "sub_merchant_limit",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sub_merchant_document",
                schema: "submerchant",
                table: "sub_merchant_document",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_sub_merchant_document_sub_merchant_sub_merchant_id",
                schema: "submerchant",
                table: "sub_merchant_document",
                column: "sub_merchant_id",
                principalSchema: "submerchant",
                principalTable: "sub_merchant",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sub_merchant_limit_sub_merchant_sub_merchant_id",
                schema: "submerchant",
                table: "sub_merchant_limit",
                column: "sub_merchant_id",
                principalSchema: "submerchant",
                principalTable: "sub_merchant",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sub_merchant_user_sub_merchant_sub_merchant_id",
                schema: "submerchant",
                table: "sub_merchant_user",
                column: "sub_merchant_id",
                principalSchema: "submerchant",
                principalTable: "sub_merchant",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
