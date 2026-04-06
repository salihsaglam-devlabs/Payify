using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AuthenticationMethodAndSecure3DTypeColumnsAddedToCardTopupRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "authentication_method",
                schema: "core",
                table: "card_topup_request",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "secure3d_type",
                schema: "core",
                table: "card_topup_request",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "authentication_method",
                schema: "core",
                table: "card_topup_request");

            migrationBuilder.DropColumn(
                name: "secure3d_type",
                schema: "core",
                table: "card_topup_request");
        }
    }
}
