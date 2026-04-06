using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AcquireBankNotOnUs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "allow_not_on_us",
                schema: "bank",
                table: "acquire_bank",
                newName: "restrict_own_card_not_on_us");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "restrict_own_card_not_on_us",
                schema: "bank",
                table: "acquire_bank",
                newName: "allow_not_on_us");
        }
    }
}
