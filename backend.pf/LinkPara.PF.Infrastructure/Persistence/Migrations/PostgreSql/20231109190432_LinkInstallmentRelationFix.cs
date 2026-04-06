using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class LinkInstallmentRelationFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "link_id1",
                schema: "link",
                table: "link_installment",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_link_installment_link_id1",
                schema: "link",
                table: "link_installment",
                column: "link_id1");

            migrationBuilder.AddForeignKey(
                name: "fk_link_installment_link_link_id1",
                schema: "link",
                table: "link_installment",
                column: "link_id1",
                principalSchema: "link",
                principalTable: "link",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_link_installment_link_link_id1",
                schema: "link",
                table: "link_installment");

            migrationBuilder.DropIndex(
                name: "ix_link_installment_link_id1",
                schema: "link",
                table: "link_installment");

            migrationBuilder.DropColumn(
                name: "link_id1",
                schema: "link",
                table: "link_installment");
        }
    }
}
