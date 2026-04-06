using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class ExternalPersonIdMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "external_person_id",
                schema: "merchant",
                table: "user",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "external_customer_id",
                schema: "core",
                table: "customer",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "external_person_id",
                schema: "core",
                table: "contact_person",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "external_person_id",
                schema: "merchant",
                table: "user");

            migrationBuilder.DropColumn(
                name: "external_customer_id",
                schema: "core",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "external_person_id",
                schema: "core",
                table: "contact_person");
        }
    }
}
