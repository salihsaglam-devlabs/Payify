using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddedIsAnnulmentColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "annulment_resource",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.AddColumn<bool>(
                name: "is_annulment",
                schema: "merchant",
                table: "merchant",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_annulment",
                schema: "merchant",
                table: "merchant");

            migrationBuilder.AddColumn<string>(
                name: "annulment_resource",
                schema: "merchant",
                table: "merchant",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
