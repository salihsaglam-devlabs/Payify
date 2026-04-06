using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class EditedBankLimitTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "valid_month",
                schema: "bank",
                table: "limit");

            migrationBuilder.DropColumn(
                name: "valid_year",
                schema: "bank",
                table: "limit");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_valid_date",
                schema: "bank",
                table: "limit",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_valid_date",
                schema: "bank",
                table: "limit");

            migrationBuilder.AddColumn<string>(
                name: "valid_month",
                schema: "bank",
                table: "limit",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "valid_year",
                schema: "bank",
                table: "limit",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);
        }
    }
}
