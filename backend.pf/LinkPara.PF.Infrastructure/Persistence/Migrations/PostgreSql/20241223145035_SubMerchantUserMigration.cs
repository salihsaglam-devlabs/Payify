using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class SubMerchantUserMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sub_merchant_user",
                schema: "submerchant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    surname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    birth_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    mobile_phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    role_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    role_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    sub_merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sub_merchant_user", x => x.id);
                    table.ForeignKey(
                        name: "fk_sub_merchant_user_sub_merchant_sub_merchant_id",
                        column: x => x.sub_merchant_id,
                        principalSchema: "submerchant",
                        principalTable: "sub_merchant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sub_merchant_user_sub_merchant_id",
                schema: "submerchant",
                table: "sub_merchant_user",
                column: "sub_merchant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sub_merchant_user",
                schema: "submerchant");
        }
    }
}
