using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class TransactionDirectionAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "tag",
                schema: "core",
                table: "transaction",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tag_title",
                schema: "core",
                table: "transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transaction_direction",
                schema: "core",
                table: "transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_transaction_type",
                schema: "core",
                table: "transaction",
                column: "transaction_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_transaction_transaction_type",
                schema: "core",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "tag_title",
                schema: "core",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "transaction_direction",
                schema: "core",
                table: "transaction");

            migrationBuilder.AlterColumn<string>(
                name: "tag",
                schema: "core",
                table: "transaction",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
