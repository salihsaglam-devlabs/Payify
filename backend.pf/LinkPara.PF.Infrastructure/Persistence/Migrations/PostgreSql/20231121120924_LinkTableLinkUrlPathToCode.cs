using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class LinkTableLinkUrlPathToCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "link_url_path",
                schema: "link",
                table: "link_transaction",
                newName: "link_code");

            migrationBuilder.RenameColumn(
                name: "link_url_path",
                schema: "link",
                table: "link",
                newName: "link_code");

            migrationBuilder.RenameIndex(
                name: "ix_link_link_url_path",
                schema: "link",
                table: "link",
                newName: "ix_link_link_code");

            migrationBuilder.AddColumn<decimal>(
                name: "commission_amount",
                schema: "link",
                table: "link_transaction",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "commission_amount",
                schema: "link",
                table: "link_transaction");

            migrationBuilder.RenameColumn(
                name: "link_code",
                schema: "link",
                table: "link_transaction",
                newName: "link_url_path");

            migrationBuilder.RenameColumn(
                name: "link_code",
                schema: "link",
                table: "link",
                newName: "link_url_path");

            migrationBuilder.RenameIndex(
                name: "ix_link_link_code",
                schema: "link",
                table: "link",
                newName: "ix_link_link_url_path");
        }
    }
}
