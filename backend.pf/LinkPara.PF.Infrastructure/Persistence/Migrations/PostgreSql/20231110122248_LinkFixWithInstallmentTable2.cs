using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class LinkFixWithInstallmentTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "link_id",
                schema: "link",
                table: "link_installment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_link_installment_link_id",
                schema: "link",
                table: "link_installment",
                column: "link_id");

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

            migrationBuilder.DropIndex(
                name: "ix_link_installment_link_id",
                schema: "link",
                table: "link_installment");

            migrationBuilder.DropColumn(
                name: "link_id",
                schema: "link",
                table: "link_installment");
        }
    }
}
