using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Identity.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedChannelToUserActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "channel",
                schema: "core",
                table: "user_login_last_activity",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "channel",
                schema: "core",
                table: "login_activity",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "channel",
                schema: "core",
                table: "user_login_last_activity");

            migrationBuilder.DropColumn(
                name: "channel",
                schema: "core",
                table: "login_activity");
        }
    }
}
