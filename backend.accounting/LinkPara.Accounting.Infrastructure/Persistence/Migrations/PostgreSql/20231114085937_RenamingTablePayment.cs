using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Accounting.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class RenamingTablePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "sender_register_invoice_id",
                schema: "core",
                table: "payment",
                newName: "sender_invoice_id");

            migrationBuilder.RenameColumn(
                name: "receiver_register_invoice_id",
                schema: "core",
                table: "payment",
                newName: "receiver_invoice_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "sender_invoice_id",
                schema: "core",
                table: "payment",
                newName: "sender_register_invoice_id");

            migrationBuilder.RenameColumn(
                name: "receiver_invoice_id",
                schema: "core",
                table: "payment",
                newName: "receiver_register_invoice_id");
        }
    }
}
