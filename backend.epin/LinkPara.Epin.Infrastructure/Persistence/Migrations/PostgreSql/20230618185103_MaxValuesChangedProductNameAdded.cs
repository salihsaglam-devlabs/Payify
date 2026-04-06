using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Epin.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MaxValuesChangedProductNameAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "product_name",
                schema: "core",
                table: "reconciliation_detail",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "summary",
                schema: "core",
                table: "brand",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(4001)",
                oldMaxLength: 4001);

            migrationBuilder.AlterColumn<string>(
                name: "image",
                schema: "core",
                table: "brand",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(4001)",
                oldMaxLength: 4001);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                schema: "core",
                table: "brand",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(4001)",
                oldMaxLength: 4001);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "product_name",
                schema: "core",
                table: "reconciliation_detail");

            migrationBuilder.AlterColumn<string>(
                name: "summary",
                schema: "core",
                table: "brand",
                type: "character varying(4001)",
                maxLength: 4001,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "image",
                schema: "core",
                table: "brand",
                type: "character varying(4001)",
                maxLength: 4001,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                schema: "core",
                table: "brand",
                type: "character varying(4001)",
                maxLength: 4001,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
