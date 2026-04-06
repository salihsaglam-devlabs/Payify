using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgrSql
{
    /// <inheritdoc />
    public partial class UpdatedMerchantPreAppHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_merchant_pre_application_history_merchant_pre_application_m",
                schema: "merchant",
                table: "merchant_pre_application_history");

            migrationBuilder.DropColumn(
                name: "pending_pos_application_id",
                schema: "merchant",
                table: "merchant_pre_application_history");

            migrationBuilder.AlterColumn<Guid>(
                name: "merchant_pre_application_id",
                schema: "merchant",
                table: "merchant_pre_application_history",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_merchant_pre_application_history_merchant_pre_application_m",
                schema: "merchant",
                table: "merchant_pre_application_history",
                column: "merchant_pre_application_id",
                principalSchema: "merchant",
                principalTable: "merchant_pre_application",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_merchant_pre_application_history_merchant_pre_application_m",
                schema: "merchant",
                table: "merchant_pre_application_history");

            migrationBuilder.AlterColumn<Guid>(
                name: "merchant_pre_application_id",
                schema: "merchant",
                table: "merchant_pre_application_history",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "pending_pos_application_id",
                schema: "merchant",
                table: "merchant_pre_application_history",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "fk_merchant_pre_application_history_merchant_pre_application_m",
                schema: "merchant",
                table: "merchant_pre_application_history",
                column: "merchant_pre_application_id",
                principalSchema: "merchant",
                principalTable: "merchant_pre_application",
                principalColumn: "id");
        }
    }
}
