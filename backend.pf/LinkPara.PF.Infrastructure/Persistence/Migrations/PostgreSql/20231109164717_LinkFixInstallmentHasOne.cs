using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class LinkFixInstallmentHasOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_link_installment_link_link_id",
                schema: "link",
                table: "link_installment");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_link_installment_link_link_id",
                schema: "link",
                table: "link_installment");

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
    }
}
