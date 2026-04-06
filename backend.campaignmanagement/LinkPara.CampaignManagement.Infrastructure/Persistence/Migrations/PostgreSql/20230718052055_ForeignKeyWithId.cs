using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.CampaignManagement.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class ForeignKeyWithId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_i_wallet_qr_code_i_wallet_card_i_wallet_card_id",
                schema: "core",
                table: "i_wallet_qr_code");

            migrationBuilder.DropIndex(
                name: "ix_i_wallet_qr_code_card_id",
                schema: "core",
                table: "i_wallet_qr_code");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_i_wallet_card_card_id",
                schema: "core",
                table: "i_wallet_card");

            migrationBuilder.AddColumn<Guid>(
                name: "i_wallet_card_id",
                schema: "core",
                table: "i_wallet_qr_code",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_i_wallet_qr_code_i_wallet_card_id",
                schema: "core",
                table: "i_wallet_qr_code",
                column: "i_wallet_card_id");

            migrationBuilder.AddForeignKey(
                name: "fk_i_wallet_qr_code_i_wallet_card_i_wallet_card_id",
                schema: "core",
                table: "i_wallet_qr_code",
                column: "i_wallet_card_id",
                principalSchema: "core",
                principalTable: "i_wallet_card",
                principalColumn: "id",
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
                name: "ix_i_wallet_qr_code_i_wallet_card_id",
                schema: "core",
                table: "i_wallet_qr_code");

            migrationBuilder.DropColumn(
                name: "i_wallet_card_id",
                schema: "core",
                table: "i_wallet_qr_code");

            migrationBuilder.AddUniqueConstraint(
                name: "ak_i_wallet_card_card_id",
                schema: "core",
                table: "i_wallet_card",
                column: "card_id");

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
    }
}
