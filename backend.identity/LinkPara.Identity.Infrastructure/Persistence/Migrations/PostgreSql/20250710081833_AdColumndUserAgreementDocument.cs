using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Identity.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AdColumndUserAgreementDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "approval_channel",
                schema: "core",
                table: "user_agreement_document",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "approval_channel",
                schema: "core",
                table: "user_agreement_document");
        }
    }
}
