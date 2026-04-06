using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.LogConsumers.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class MigrationFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "channel",
                schema: "core",
                table: "audit_log",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "correlation_id",
                schema: "core",
                table: "audit_log",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "correlation_id",
                schema: "core",
                table: "audit_log");

            migrationBuilder.AlterColumn<string>(
                name: "channel",
                schema: "core",
                table: "audit_log",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);
        }
    }
}
