using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Card.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class CustomerWalletCardName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "card_name",
                schema: "core",
                table: "customer_wallet_card",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "card_name",
                schema: "core",
                table: "customer_wallet_card");
        }
    }
}
