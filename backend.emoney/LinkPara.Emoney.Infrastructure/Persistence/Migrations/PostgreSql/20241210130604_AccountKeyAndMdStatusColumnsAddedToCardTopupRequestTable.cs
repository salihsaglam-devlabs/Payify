using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AccountKeyAndMdStatusColumnsAddedToCardTopupRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "account_key",
                schema: "core",
                table: "card_topup_request",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "md_status",
                schema: "core",
                table: "card_topup_request",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "account_key",
                schema: "core",
                table: "card_topup_request");

            migrationBuilder.DropColumn(
                name: "md_status",
                schema: "core",
                table: "card_topup_request");
        }
    }
}
