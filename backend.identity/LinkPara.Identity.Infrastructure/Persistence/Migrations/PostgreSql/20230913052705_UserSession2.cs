using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Identity.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class UserSession2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_session_user_id",
                schema: "core",
                table: "user_session");

            migrationBuilder.CreateIndex(
                name: "ix_user_session_user_id",
                schema: "core",
                table: "user_session",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_session_user_id",
                schema: "core",
                table: "user_session");

            migrationBuilder.CreateIndex(
                name: "ix_user_session_user_id",
                schema: "core",
                table: "user_session",
                column: "user_id",
                unique: true);
        }
    }
}
