using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Identity.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class UserSessionAddIndexDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_user_session_refresh_token_expiration",
                schema: "core",
                table: "user_session",
                column: "refresh_token_expiration");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_session_refresh_token_expiration",
                schema: "core",
                table: "user_session");
        }
    }
}
