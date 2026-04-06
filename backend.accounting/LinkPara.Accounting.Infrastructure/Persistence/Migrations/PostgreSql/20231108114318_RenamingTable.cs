using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Accounting.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class RenamingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "register_invoice",
                schema: "core");

            migrationBuilder.CreateTable(
                name: "invoice",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    total_commission = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_bsmv = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoice", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invoice",
                schema: "core");

            migrationBuilder.CreateTable(
                name: "register_invoice",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    total_bsmv = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_commission = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_register_invoice", x => x.id);
                });
        }
    }
}
