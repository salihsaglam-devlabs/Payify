using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedIsTerminalNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_blockage_detail_blockage_merchant_blockage_id",
                schema: "merchant",
                table: "blockage_detail");

            migrationBuilder.AddColumn<bool>(
                name: "is_terminal_notification",
                schema: "merchant",
                table: "vpos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "merchant_blockage_id",
                schema: "merchant",
                table: "blockage_detail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_blockage_detail_blockage_merchant_blockage_id",
                schema: "merchant",
                table: "blockage_detail",
                column: "merchant_blockage_id",
                principalSchema: "merchant",
                principalTable: "blockage",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_blockage_detail_blockage_merchant_blockage_id",
                schema: "merchant",
                table: "blockage_detail");

            migrationBuilder.DropColumn(
                name: "is_terminal_notification",
                schema: "merchant",
                table: "vpos");

            migrationBuilder.AlterColumn<Guid>(
                name: "merchant_blockage_id",
                schema: "merchant",
                table: "blockage_detail",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "fk_blockage_detail_blockage_merchant_blockage_id",
                schema: "merchant",
                table: "blockage_detail",
                column: "merchant_blockage_id",
                principalSchema: "merchant",
                principalTable: "blockage",
                principalColumn: "id");
        }
    }
}
