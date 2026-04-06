using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class ChargebackWithoutCommissionMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_transaction",
                schema: "posting",
                table: "transaction");

            migrationBuilder.AddColumn<string>(
                name: "conversation_id",
                schema: "posting",
                table: "transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "deduction_amount_with_commission",
                schema: "merchant",
                table: "merchant_deduction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_transaction",
                schema: "posting",
                table: "transaction",
                column: "id");

            migrationBuilder.CreateTable(
                name: "posting_additional_transaction",
                schema: "posting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posting_additional_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_posting_additional_transaction_transaction_id",
                        column: x => x.id,
                        principalSchema: "posting",
                        principalTable: "transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "posting_additional_transaction",
                schema: "posting");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transaction",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "conversation_id",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "deduction_amount_with_commission",
                schema: "merchant",
                table: "merchant_deduction");

            migrationBuilder.AddPrimaryKey(
                name: "pk_transaction",
                schema: "posting",
                table: "transaction",
                column: "id");
        }
    }
}
