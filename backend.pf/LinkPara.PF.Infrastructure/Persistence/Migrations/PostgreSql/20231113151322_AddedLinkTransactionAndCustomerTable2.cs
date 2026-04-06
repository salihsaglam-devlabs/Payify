using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedLinkTransactionAndCustomerTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "link_id",
                schema: "link",
                table: "link_customer",
                newName: "link_transaction_id");

            migrationBuilder.AddColumn<Guid>(
                name: "customer_id",
                schema: "link",
                table: "link_transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "customer_id",
                schema: "link",
                table: "link_transaction");

            migrationBuilder.RenameColumn(
                name: "link_transaction_id",
                schema: "link",
                table: "link_customer",
                newName: "link_id");
        }
    }
}
