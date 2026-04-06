using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Approval.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class BodyMaxLenght : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "body",
                schema: "core",
                table: "request",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4001)",
                oldMaxLength: 4001,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "body",
                schema: "core",
                table: "request",
                type: "character varying(4001)",
                maxLength: 4001,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
