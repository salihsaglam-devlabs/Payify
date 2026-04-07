using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Card.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class CustomerWalletCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "core",
                table: "customer_wallet_card");

            migrationBuilder.AddColumn<string>(
                name: "card_status",
                schema: "core",
                table: "customer_wallet_card",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "card_status",
                schema: "core",
                table: "customer_wallet_card");

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                schema: "core",
                table: "customer_wallet_card",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
