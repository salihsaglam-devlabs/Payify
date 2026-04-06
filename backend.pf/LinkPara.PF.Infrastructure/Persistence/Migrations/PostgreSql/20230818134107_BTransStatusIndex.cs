using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class BTransStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "vpos_id",
                schema: "posting",
                table: "transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_balance_money_transfer_status_b_trans_status",
                schema: "posting",
                table: "balance",
                columns: new[] { "money_transfer_status", "b_trans_status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_balance_money_transfer_status_b_trans_status",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "vpos_id",
                schema: "posting",
                table: "transaction");
        }
    }
}
