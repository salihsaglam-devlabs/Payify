using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class DueProfileMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_merchant_deduction_merchant_transaction_merchant_transactio",
                schema: "merchant",
                table: "merchant_deduction");

            migrationBuilder.DropIndex(
                name: "ix_merchant_deduction_merchant_transaction_id",
                schema: "merchant",
                table: "merchant_deduction");

            migrationBuilder.DropColumn(
                name: "amount",
                schema: "merchant",
                table: "merchant_due");

            migrationBuilder.DropColumn(
                name: "currency",
                schema: "merchant",
                table: "merchant_due");

            migrationBuilder.DropColumn(
                name: "occurence_interval",
                schema: "merchant",
                table: "merchant_due");

            migrationBuilder.RenameColumn(
                name: "last_execution_date",
                schema: "merchant",
                table: "merchant_due",
                newName: "execution_date");

            migrationBuilder.AlterColumn<string>(
                name: "vpos_name",
                schema: "merchant",
                table: "transaction",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "provision_number",
                schema: "merchant",
                table: "transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "due_profile_id",
                schema: "merchant",
                table: "merchant_due",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "commission_from_customer",
                schema: "hpp",
                table: "hosted_payment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "due_profile",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    due_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    occurence_interval = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_due_profile", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_merchant_due_due_profile_id",
                schema: "merchant",
                table: "merchant_due",
                column: "due_profile_id");

            migrationBuilder.AddForeignKey(
                name: "fk_merchant_due_due_profile_due_profile_id",
                schema: "merchant",
                table: "merchant_due",
                column: "due_profile_id",
                principalSchema: "core",
                principalTable: "due_profile",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_merchant_due_due_profile_due_profile_id",
                schema: "merchant",
                table: "merchant_due");

            migrationBuilder.DropTable(
                name: "due_profile",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "ix_merchant_due_due_profile_id",
                schema: "merchant",
                table: "merchant_due");

            migrationBuilder.DropColumn(
                name: "due_profile_id",
                schema: "merchant",
                table: "merchant_due");

            migrationBuilder.DropColumn(
                name: "commission_from_customer",
                schema: "hpp",
                table: "hosted_payment");

            migrationBuilder.RenameColumn(
                name: "execution_date",
                schema: "merchant",
                table: "merchant_due",
                newName: "last_execution_date");

            migrationBuilder.AlterColumn<string>(
                name: "vpos_name",
                schema: "merchant",
                table: "transaction",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "provision_number",
                schema: "merchant",
                table: "transaction",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "amount",
                schema: "merchant",
                table: "merchant_due",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "currency",
                schema: "merchant",
                table: "merchant_due",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "occurence_interval",
                schema: "merchant",
                table: "merchant_due",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_deduction_merchant_transaction_id",
                schema: "merchant",
                table: "merchant_deduction",
                column: "merchant_transaction_id");

            migrationBuilder.AddForeignKey(
                name: "fk_merchant_deduction_merchant_transaction_merchant_transactio",
                schema: "merchant",
                table: "merchant_deduction",
                column: "merchant_transaction_id",
                principalSchema: "merchant",
                principalTable: "transaction",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
