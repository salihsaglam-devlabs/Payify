using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class RenameAnnulmentResourceColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "resource_annulment",
                schema: "merchant",
                table: "merchant",
                newName: "annulment_resource");

            migrationBuilder.RenameColumn(
                name: "additional_info_annulment",
                schema: "merchant",
                table: "merchant",
                newName: "annulment_additional_info");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "annulment_resource",
                schema: "merchant",
                table: "merchant",
                newName: "resource_annulment");

            migrationBuilder.RenameColumn(
                name: "annulment_additional_info",
                schema: "merchant",
                table: "merchant",
                newName: "additional_info_annulment");
        }
    }
}
