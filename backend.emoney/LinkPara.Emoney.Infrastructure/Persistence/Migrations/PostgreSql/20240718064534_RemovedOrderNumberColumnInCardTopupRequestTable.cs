using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovedOrderNumberColumnInCardTopupRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "order_number",
                schema: "core",
                table: "card_topup_request");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "order_number",
                schema: "core",
                table: "card_topup_request",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
