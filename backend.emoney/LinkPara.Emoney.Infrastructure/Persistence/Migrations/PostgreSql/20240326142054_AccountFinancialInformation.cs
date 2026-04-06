using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AccountFinancialInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_financial_information",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    income_source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    income_information = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    monthly_transaction_volume = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    monthly_transaction_count = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_financial_information", x => x.id);
                    table.ForeignKey(
                        name: "fk_account_financial_information_account_account_id",
                        column: x => x.account_id,
                        principalSchema: "core",
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_financial_information_account_id",
                schema: "core",
                table: "account_financial_information",
                column: "account_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_financial_information",
                schema: "core");
        }
    }
}
