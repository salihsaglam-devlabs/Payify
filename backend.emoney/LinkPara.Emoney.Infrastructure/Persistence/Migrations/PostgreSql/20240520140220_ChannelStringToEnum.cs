using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class ChannelStringToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "land_phone_code",
                schema: "core",
                table: "company_pool");

            migrationBuilder.RenameColumn(
                name: "land_phone_number",
                schema: "core",
                table: "company_pool",
                newName: "land_phone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "land_phone",
                schema: "core",
                table: "company_pool",
                newName: "land_phone_number");

            migrationBuilder.AddColumn<string>(
                name: "land_phone_code",
                schema: "core",
                table: "company_pool",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");
        }
    }
}
