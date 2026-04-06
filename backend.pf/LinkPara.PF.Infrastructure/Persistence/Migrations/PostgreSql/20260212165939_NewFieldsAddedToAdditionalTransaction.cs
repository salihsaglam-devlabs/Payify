using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class NewFieldsAddedToAdditionalTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "merchant_physical_pos_id",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "pf_transaction_source",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "VirtualPos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "merchant_physical_pos_id",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "pf_transaction_source",
                schema: "posting",
                table: "posting_additional_transaction");
        }
    }
}
