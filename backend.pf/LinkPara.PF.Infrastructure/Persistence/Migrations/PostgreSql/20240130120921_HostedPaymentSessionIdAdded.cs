using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class HostedPaymentSessionIdAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "three_d_session_id",
                schema: "link",
                table: "link_transaction",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "three_d_session_id",
                schema: "hpp",
                table: "hosted_payment_transaction",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "return_url",
                schema: "hpp",
                table: "hosted_payment",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "three_d_session_id",
                schema: "link",
                table: "link_transaction");

            migrationBuilder.DropColumn(
                name: "three_d_session_id",
                schema: "hpp",
                table: "hosted_payment_transaction");

            migrationBuilder.DropColumn(
                name: "return_url",
                schema: "hpp",
                table: "hosted_payment");
        }
    }
}
