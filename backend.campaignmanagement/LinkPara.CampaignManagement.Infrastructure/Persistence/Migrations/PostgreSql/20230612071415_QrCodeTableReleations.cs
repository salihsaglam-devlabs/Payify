using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.CampaignManagement.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class QrCodeTableReleations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_i_wallet_qr_code_i_wallet_card_i_wallet_card_id1",
                schema: "core",
                table: "i_wallet_qr_code");

            migrationBuilder.DropIndex(
                name: "ix_i_wallet_qr_code_i_wallet_card_id",
                schema: "core",
                table: "i_wallet_qr_code");

            migrationBuilder.DropColumn(
                name: "i_wallet_card_id",
                schema: "core",
                table: "i_wallet_qr_code");

            migrationBuilder.CreateIndex(
                name: "ix_i_wallet_qr_code_card_id",
                schema: "core",
                table: "i_wallet_qr_code",
                column: "card_id");

            migrationBuilder.AddForeignKey(
                name: "fk_i_wallet_qr_code_i_wallet_card_i_wallet_card_id",
                schema: "core",
                table: "i_wallet_qr_code",
                column: "card_id",
                principalSchema: "core",
                principalTable: "i_wallet_card",
                principalColumn: "card_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_i_wallet_qr_code_i_wallet_card_i_wallet_card_id",
                schema: "core",
                table: "i_wallet_qr_code");

            migrationBuilder.DropIndex(
                name: "ix_i_wallet_qr_code_card_id",
                schema: "core",
                table: "i_wallet_qr_code");

            migrationBuilder.AddColumn<int>(
                name: "i_wallet_card_id",
                schema: "core",
                table: "i_wallet_qr_code",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_i_wallet_qr_code_i_wallet_card_id",
                schema: "core",
                table: "i_wallet_qr_code",
                column: "i_wallet_card_id");

            migrationBuilder.AddForeignKey(
                name: "fk_i_wallet_qr_code_i_wallet_card_i_wallet_card_id1",
                schema: "core",
                table: "i_wallet_qr_code",
                column: "i_wallet_card_id",
                principalSchema: "core",
                principalTable: "i_wallet_card",
                principalColumn: "card_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
