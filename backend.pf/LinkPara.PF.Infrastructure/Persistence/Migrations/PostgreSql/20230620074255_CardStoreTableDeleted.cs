using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class CardStoreTableDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_token_store_card_store_id",
                schema: "card",
                table: "token");

            migrationBuilder.DropTable(
                name: "store",
                schema: "card");

            migrationBuilder.DropIndex(
                name: "ix_token_card_store_id",
                schema: "card",
                table: "token");

            migrationBuilder.DropColumn(
                name: "card_store_id",
                schema: "card",
                table: "token");

            migrationBuilder.AddColumn<string>(
                name: "card_number_encrypted",
                schema: "card",
                table: "token",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "expire_date_encrypted",
                schema: "card",
                table: "token",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "card_number_encrypted",
                schema: "card",
                table: "token");

            migrationBuilder.DropColumn(
                name: "expire_date_encrypted",
                schema: "card",
                table: "token");

            migrationBuilder.AddColumn<Guid>(
                name: "card_store_id",
                schema: "card",
                table: "token",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "store",
                schema: "card",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_number_encrypted = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    card_number_hashed = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    expire_date_encrypted = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_store", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_token_card_store_id",
                schema: "card",
                table: "token",
                column: "card_store_id");

            migrationBuilder.AddForeignKey(
                name: "fk_token_store_card_store_id",
                schema: "card",
                table: "token",
                column: "card_store_id",
                principalSchema: "card",
                principalTable: "store",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
