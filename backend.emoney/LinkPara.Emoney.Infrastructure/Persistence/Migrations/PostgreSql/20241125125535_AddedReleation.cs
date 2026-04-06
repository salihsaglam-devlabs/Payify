using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedReleation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_account_custom_tier_account_id",
                schema: "limit",
                table: "account_custom_tier",
                column: "account_id");

            migrationBuilder.AddForeignKey(
                name: "fk_account_custom_tier_account_account_id",
                schema: "limit",
                table: "account_custom_tier",
                column: "account_id",
                principalSchema: "core",
                principalTable: "account",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_account_custom_tier_account_account_id",
                schema: "limit",
                table: "account_custom_tier");

            migrationBuilder.DropIndex(
                name: "ix_account_custom_tier_account_id",
                schema: "limit",
                table: "account_custom_tier");
        }
    }
}
