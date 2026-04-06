using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Accounting.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class RetryCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "failed_payment_retry_count",
                schema: "core",
                table: "payment",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "failed_payment_retry_count",
                schema: "core",
                table: "payment");
        }
    }
}
