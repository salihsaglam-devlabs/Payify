using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostingBillsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "bill_date",
                schema: "posting",
                table: "posting_bill",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "bill_month",
                schema: "posting",
                table: "posting_bill",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "currency",
                schema: "posting",
                table: "posting_bill",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bill_date",
                schema: "posting",
                table: "posting_bill");

            migrationBuilder.DropColumn(
                name: "bill_month",
                schema: "posting",
                table: "posting_bill");

            migrationBuilder.DropColumn(
                name: "currency",
                schema: "posting",
                table: "posting_bill");
        }
    }
}
