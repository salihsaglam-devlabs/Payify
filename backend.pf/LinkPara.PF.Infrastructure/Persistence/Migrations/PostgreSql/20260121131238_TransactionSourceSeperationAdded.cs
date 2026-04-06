using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class TransactionSourceSeperationAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "transaction_source",
                schema: "posting",
                table: "transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "VirtualPos");

            migrationBuilder.AddColumn<string>(
                name: "transaction_source",
                schema: "merchant",
                table: "transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "VirtualPos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "transaction_source",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "transaction_source",
                schema: "merchant",
                table: "transaction");
        }
    }
}
