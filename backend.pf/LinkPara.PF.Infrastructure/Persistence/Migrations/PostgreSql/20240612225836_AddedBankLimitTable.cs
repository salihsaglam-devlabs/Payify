using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedBankLimitTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "limit",
                schema: "bank",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    acquire_bank_id = table.Column<Guid>(type: "uuid", nullable: false),
                    monthly_limit_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    margin_ratio = table.Column<int>(type: "integer", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    bank_limit_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    valid_month = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    valid_year = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_limit", x => x.id);
                    table.ForeignKey(
                        name: "fk_limit_acquire_bank_acquire_bank_id",
                        column: x => x.acquire_bank_id,
                        principalSchema: "bank",
                        principalTable: "acquire_bank",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_limit_acquire_bank_id",
                schema: "bank",
                table: "limit",
                column: "acquire_bank_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "limit",
                schema: "bank");
        }
    }
}
