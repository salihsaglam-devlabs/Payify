using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedPosTypeToMerchant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "pos_type",
                schema: "merchant",
                table: "pool",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Virtual");

            migrationBuilder.AddColumn<string>(
                name: "pos_type",
                schema: "merchant",
                table: "merchant",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Virtual");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pos_type",
                schema: "merchant",
                table: "pool");

            migrationBuilder.DropColumn(
                name: "pos_type",
                schema: "merchant",
                table: "merchant");
        }
    }
}
