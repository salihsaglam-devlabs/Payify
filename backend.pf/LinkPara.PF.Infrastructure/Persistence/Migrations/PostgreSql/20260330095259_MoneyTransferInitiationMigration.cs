using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MoneyTransferInitiationMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "processing_id",
                schema: "merchant",
                table: "merchant_deduction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "processing_started_at",
                schema: "merchant",
                table: "merchant_deduction",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "processing_id",
                schema: "posting",
                table: "balance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "processing_started_at",
                schema: "posting",
                table: "balance",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "processing_id",
                schema: "merchant",
                table: "merchant_deduction");

            migrationBuilder.DropColumn(
                name: "processing_started_at",
                schema: "merchant",
                table: "merchant_deduction");

            migrationBuilder.DropColumn(
                name: "processing_id",
                schema: "posting",
                table: "balance");

            migrationBuilder.DropColumn(
                name: "processing_started_at",
                schema: "posting",
                table: "balance");
        }
    }
}
