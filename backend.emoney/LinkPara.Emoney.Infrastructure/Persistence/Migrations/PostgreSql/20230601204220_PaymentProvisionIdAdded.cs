using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PaymentProvisionIdAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "payment_provision_id",
                schema: "core",
                table: "provision",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_provision_payment_provision_id",
                schema: "core",
                table: "provision",
                column: "payment_provision_id");

            migrationBuilder.AddForeignKey(
                name: "fk_provision_provision_payment_provision_id",
                schema: "core",
                table: "provision",
                column: "payment_provision_id",
                principalSchema: "core",
                principalTable: "provision",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_provision_provision_payment_provision_id",
                schema: "core",
                table: "provision");

            migrationBuilder.DropIndex(
                name: "ix_provision_payment_provision_id",
                schema: "core",
                table: "provision");

            migrationBuilder.DropColumn(
                name: "payment_provision_id",
                schema: "core",
                table: "provision");
        }
    }
}
