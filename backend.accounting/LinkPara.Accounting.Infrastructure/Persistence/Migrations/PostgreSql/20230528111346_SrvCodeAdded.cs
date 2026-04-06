using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Accounting.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class SrvCodeAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "srv_code",
                schema: "core",
                table: "template",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "srv_code",
                schema: "core",
                table: "template");
        }
    }
}
