using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class LinkUrlGenerateFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_link_installment_link_link_id",
                schema: "link",
                table: "link_installment");

            migrationBuilder.AddColumn<string>(
                name: "link_payment_status",
                schema: "link",
                table: "link",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "fk_link_installment_link_link_id",
                schema: "link",
                table: "link_installment",
                column: "link_id",
                principalSchema: "link",
                principalTable: "link",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_link_installment_link_link_id",
                schema: "link",
                table: "link_installment");

            migrationBuilder.DropColumn(
                name: "link_payment_status",
                schema: "link",
                table: "link");

            migrationBuilder.AddForeignKey(
                name: "fk_link_installment_link_link_id",
                schema: "link",
                table: "link_installment",
                column: "link_id",
                principalSchema: "link",
                principalTable: "link",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
