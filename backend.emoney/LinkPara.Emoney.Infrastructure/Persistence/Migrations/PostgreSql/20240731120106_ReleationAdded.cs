using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class ReleationAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "account_id",
                schema: "core",
                table: "company_pool",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "file_name",
                schema: "core",
                table: "bulk_transfer",
                type: "character varying(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(400)",
                oldMaxLength: 400);

            migrationBuilder.CreateIndex(
                name: "ix_company_pool_account_id",
                schema: "core",
                table: "company_pool",
                column: "account_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_company_pool_account_account_id",
                schema: "core",
                table: "company_pool",
                column: "account_id",
                principalSchema: "core",
                principalTable: "account",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_company_pool_account_account_id",
                schema: "core",
                table: "company_pool");

            migrationBuilder.DropIndex(
                name: "ix_company_pool_account_id",
                schema: "core",
                table: "company_pool");

            migrationBuilder.DropColumn(
                name: "account_id",
                schema: "core",
                table: "company_pool");

            migrationBuilder.AlterColumn<string>(
                name: "file_name",
                schema: "core",
                table: "bulk_transfer",
                type: "character varying(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(400)",
                oldMaxLength: 400,
                oldNullable: true);
        }
    }
}
