using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Emoney.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class UpdatePricingProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "card_type",
                schema: "core",
                table: "pricing_profile",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "card_type",
                schema: "core",
                table: "pricing_profile");
        }
    }
}
