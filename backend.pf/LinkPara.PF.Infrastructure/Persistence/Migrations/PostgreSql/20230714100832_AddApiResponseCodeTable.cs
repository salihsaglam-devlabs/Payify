using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddApiResponseCodeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_api_log_merchant_merchant_id",
                schema: "merchant",
                table: "api_log");

            migrationBuilder.DropForeignKey(
                name: "fk_api_validation_log_merchant_merchant_id",
                schema: "merchant",
                table: "api_validation_log");

            migrationBuilder.DropPrimaryKey(
                name: "pk_api_validation_log",
                schema: "merchant",
                table: "api_validation_log");

            migrationBuilder.DropPrimaryKey(
                name: "pk_api_log",
                schema: "merchant",
                table: "api_log");

            migrationBuilder.EnsureSchema(
                name: "api");

            migrationBuilder.RenameTable(
                name: "api_validation_log",
                schema: "merchant",
                newName: "validation_log",
                newSchema: "api");

            migrationBuilder.RenameTable(
                name: "api_log",
                schema: "merchant",
                newName: "log",
                newSchema: "api");

            migrationBuilder.RenameIndex(
                name: "ix_response_code_merchant_response_code_id",
                schema: "bank",
                table: "response_code",
                newName: "ix_response_code_merchant_response_code_id1");

            migrationBuilder.RenameIndex(
                name: "ix_api_validation_log_merchant_id",
                schema: "api",
                table: "validation_log",
                newName: "ix_validation_log_merchant_id");

            migrationBuilder.RenameIndex(
                name: "ix_api_log_merchant_id",
                schema: "api",
                table: "log",
                newName: "ix_log_merchant_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_validation_log",
                schema: "api",
                table: "validation_log",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_log",
                schema: "api",
                table: "log",
                column: "id");

            migrationBuilder.CreateTable(
                name: "response_code",
                schema: "api",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    response_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    merchant_response_code_id = table.Column<Guid>(type: "uuid", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_response_code", x => x.id);
                    table.ForeignKey(
                        name: "fk_response_code_merchant_response_code_merchant_response_code",
                        column: x => x.merchant_response_code_id,
                        principalSchema: "merchant",
                        principalTable: "response_code",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_response_code_merchant_response_code_id",
                schema: "api",
                table: "response_code",
                column: "merchant_response_code_id");

            migrationBuilder.AddForeignKey(
                name: "fk_log_merchant_merchant_id",
                schema: "api",
                table: "log",
                column: "merchant_id",
                principalSchema: "merchant",
                principalTable: "merchant",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_validation_log_merchant_merchant_id",
                schema: "api",
                table: "validation_log",
                column: "merchant_id",
                principalSchema: "merchant",
                principalTable: "merchant",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_log_merchant_merchant_id",
                schema: "api",
                table: "log");

            migrationBuilder.DropForeignKey(
                name: "fk_validation_log_merchant_merchant_id",
                schema: "api",
                table: "validation_log");

            migrationBuilder.DropTable(
                name: "response_code",
                schema: "api");

            migrationBuilder.DropPrimaryKey(
                name: "pk_validation_log",
                schema: "api",
                table: "validation_log");

            migrationBuilder.DropPrimaryKey(
                name: "pk_log",
                schema: "api",
                table: "log");

            migrationBuilder.RenameTable(
                name: "validation_log",
                schema: "api",
                newName: "api_validation_log",
                newSchema: "merchant");

            migrationBuilder.RenameTable(
                name: "log",
                schema: "api",
                newName: "api_log",
                newSchema: "merchant");

            migrationBuilder.RenameIndex(
                name: "ix_response_code_merchant_response_code_id1",
                schema: "bank",
                table: "response_code",
                newName: "ix_response_code_merchant_response_code_id");

            migrationBuilder.RenameIndex(
                name: "ix_validation_log_merchant_id",
                schema: "merchant",
                table: "api_validation_log",
                newName: "ix_api_validation_log_merchant_id");

            migrationBuilder.RenameIndex(
                name: "ix_log_merchant_id",
                schema: "merchant",
                table: "api_log",
                newName: "ix_api_log_merchant_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_api_validation_log",
                schema: "merchant",
                table: "api_validation_log",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_api_log",
                schema: "merchant",
                table: "api_log",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_api_log_merchant_merchant_id",
                schema: "merchant",
                table: "api_log",
                column: "merchant_id",
                principalSchema: "merchant",
                principalTable: "merchant",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_api_validation_log_merchant_merchant_id",
                schema: "merchant",
                table: "api_validation_log",
                column: "merchant_id",
                principalSchema: "merchant",
                principalTable: "merchant",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
