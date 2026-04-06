using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedBankHealthCheckTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "health_check",
                schema: "bank",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    acquire_bank_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_check_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    total_transaction_count = table.Column<int>(type: "integer", nullable: false),
                    fail_transaction_count = table.Column<int>(type: "integer", nullable: false),
                    fail_transaction_rate = table.Column<int>(type: "integer", nullable: false),
                    health_check_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bank_status = table.Column<bool>(type: "boolean", nullable: false),
                    is_health_check_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_health_check", x => x.id);
                    table.ForeignKey(
                        name: "fk_health_check_acquire_bank_acquire_bank_id",
                        column: x => x.acquire_bank_id,
                        principalSchema: "bank",
                        principalTable: "acquire_bank",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_health_check_acquire_bank_id",
                schema: "bank",
                table: "health_check",
                column: "acquire_bank_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "health_check",
                schema: "bank");
        }
    }
}
