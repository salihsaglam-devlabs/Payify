using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Fraud.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedChannelTypeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "channel_type",
                schema: "core",
                table: "search_log",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "channel_type",
                schema: "core",
                table: "search_log");
        }
    }
}
