using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostingBalanceMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_balance_posting_bank_balance_posting_bank_balance_id",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropIndex(
                name: "ix_balance_posting_bank_balance_id",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "posting_bank_balance_id",
                schema: "posting",
                table: "balance");

            migrationBuilder.AddColumn<Guid>(
                name: "posting_balance_id",
                schema: "posting",
                table: "bank_balance",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_bank_balance_posting_balance_id",
                schema: "posting",
                table: "bank_balance",
                column: "posting_balance_id");

            migrationBuilder.AddForeignKey(
                name: "fk_bank_balance_balance_posting_balance_id",
                schema: "posting",
                table: "bank_balance",
                column: "posting_balance_id",
                principalSchema: "posting",
                principalTable: "balance",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_bank_balance_balance_posting_balance_id",
                schema: "posting",
                table: "bank_balance");

            migrationBuilder.DropIndex(
                name: "ix_bank_balance_posting_balance_id",
                schema: "posting",
                table: "bank_balance");

            migrationBuilder.DropColumn(
                name: "posting_balance_id",
                schema: "posting",
                table: "bank_balance");

            migrationBuilder.AddColumn<Guid>(
                name: "posting_bank_balance_id",
                schema: "posting",
                table: "balance",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_balance_posting_bank_balance_id",
                schema: "posting",
                table: "balance",
                column: "posting_bank_balance_id");

            migrationBuilder.AddForeignKey(
                name: "fk_balance_posting_bank_balance_posting_bank_balance_id",
                schema: "posting",
                table: "balance",
                column: "posting_bank_balance_id",
                principalSchema: "posting",
                principalTable: "bank_balance",
                principalColumn: "id");
        }
    }
}
