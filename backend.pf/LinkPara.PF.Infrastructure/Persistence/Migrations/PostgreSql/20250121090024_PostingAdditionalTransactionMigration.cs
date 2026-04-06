using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.PF.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostingAdditionalTransactionMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_posting_additional_transaction_transaction_id",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transaction",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_posting_additional_transaction",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.AddColumn<int>(
                name: "acquire_bank_code",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "amount",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "amount_without_bank_commission",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "amount_without_commissions",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "b_trans_status",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "bank_commission_amount",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "bank_commission_rate",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "numeric(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "batch_status",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "blockage_status",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "card_number",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "conversation_id",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "create_date",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "currency",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "installment_count",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "last_modified_by",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "merchant_id",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "merchant_transaction_id",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "old_payment_date",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "order_id",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "payment_date",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "pf_commission_amount",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "pf_commission_rate",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "numeric(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "pf_net_commission_amount",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "pf_per_transaction_fee",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "point_amount",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "posting_balance_id",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "posting_bank_balance_id",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "posting_date",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "pricing_profile_item_id",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "pricing_profile_number",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "record_status",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "transaction_date",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "transaction_end_date",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "transaction_start_date",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "transaction_type",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "update_date",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "vpos_id",
                schema: "posting",
                table: "posting_additional_transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "pk_transaction",
                schema: "posting",
                table: "transaction",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_posting_additional_transaction",
                schema: "posting",
                table: "posting_additional_transaction",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_posting_additional_transaction_batch_status_record_status",
                schema: "posting",
                table: "posting_additional_transaction",
                columns: new[] { "batch_status", "record_status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_transaction",
                schema: "posting",
                table: "transaction");

            migrationBuilder.DropPrimaryKey(
                name: "pk_posting_additional_transaction",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropIndex(
                name: "ix_posting_additional_transaction_batch_status_record_status",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "acquire_bank_code",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "amount",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "amount_without_bank_commission",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "amount_without_commissions",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "b_trans_status",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "bank_commission_amount",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "bank_commission_rate",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "batch_status",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "blockage_status",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "card_number",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "conversation_id",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "create_date",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "created_by",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "currency",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "installment_count",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "last_modified_by",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "merchant_id",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "merchant_transaction_id",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "old_payment_date",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "order_id",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "payment_date",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "pf_commission_amount",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "pf_commission_rate",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "pf_net_commission_amount",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "pf_per_transaction_fee",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "point_amount",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "posting_balance_id",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "posting_bank_balance_id",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "posting_date",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "pricing_profile_item_id",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "pricing_profile_number",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "record_status",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "transaction_date",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "transaction_end_date",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "transaction_start_date",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "transaction_type",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "update_date",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.DropColumn(
                name: "vpos_id",
                schema: "posting",
                table: "posting_additional_transaction");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transaction",
                schema: "posting",
                table: "transaction",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_posting_additional_transaction",
                schema: "posting",
                table: "posting_additional_transaction",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_posting_additional_transaction_transaction_id",
                schema: "posting",
                table: "posting_additional_transaction",
                column: "id",
                principalSchema: "posting",
                principalTable: "transaction",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
