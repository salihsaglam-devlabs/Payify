using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class CreatedNaceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "nace_code",
                schema: "merchant",
                table: "merchant",
                type: "character varying(10)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nace",
                schema: "merchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sector_code = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    sector_description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    profession_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    profession_description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nace", x => x.id);
                    table.UniqueConstraint("ak_nace_code", x => x.code);
                });

            migrationBuilder.CreateIndex(
                name: "ix_merchant_nace_code",
                schema: "merchant",
                table: "merchant",
                column: "nace_code");

            migrationBuilder.AddForeignKey(
                name: "fk_merchant_nace_nace_code",
                schema: "merchant",
                table: "merchant",
                column: "nace_code",
                principalSchema: "merchant",
                principalTable: "nace",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_merchant_nace_nace_code",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropTable(
                name: "nace",
                schema: "merchant");

            migrationBuilder.DropIndex(
                name: "ix_merchant_nace_code",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "nace_code",
                schema: "merchant",
                table: "merchant");
        }
    }
}
