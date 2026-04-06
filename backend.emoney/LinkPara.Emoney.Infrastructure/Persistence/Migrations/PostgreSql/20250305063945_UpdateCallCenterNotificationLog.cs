using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class UpdateCallCenterNotificationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "call_center_notification_log ",
                schema: "core",
                newName: "call_center_notification_log",
                newSchema: "core");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "call_center_notification_log",
                schema: "core",
                newName: "call_center_notification_log ",
                newSchema: "core");
        }
    }
}
