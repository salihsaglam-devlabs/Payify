using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedNewIksFieldsToMerchant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "branch_count",
                schema: "merchant",
                table: "merchant",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "business_activity",
                schema: "merchant",
                table: "merchant",
                type: "character varying(140)",
                maxLength: 140,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "business_model",
                schema: "merchant",
                table: "merchant",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "employee_count",
                schema: "merchant",
                table: "merchant",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "establishment_date",
                schema: "merchant",
                table: "merchant",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "branch_count",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "business_activity",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "business_model",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "employee_count",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "establishment_date",
                schema: "merchant",
                table: "merchant");
        }
    }
}
