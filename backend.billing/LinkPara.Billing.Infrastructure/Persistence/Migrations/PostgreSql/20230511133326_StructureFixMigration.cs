using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkPara.Billing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StructureFixMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_institutio_mapping_institution_institution_id",
                schema: "core",
                table: "institutio_mapping");

            migrationBuilder.DropForeignKey(
                name: "fk_institutio_mapping_vendor_vendor_id",
                schema: "core",
                table: "institutio_mapping");

            migrationBuilder.DropForeignKey(
                name: "fk_institutio_summary_institution_institution_id",
                schema: "core",
                table: "institutio_summary");

            migrationBuilder.DropForeignKey(
                name: "fk_institutio_summary_vendor_vendor_id",
                schema: "core",
                table: "institutio_summary");

            migrationBuilder.DropPrimaryKey(
                name: "pk_institutio_summary",
                schema: "core",
                table: "institutio_summary");

            migrationBuilder.DropPrimaryKey(
                name: "pk_institutio_mapping",
                schema: "core",
                table: "institutio_mapping");

            migrationBuilder.RenameTable(
                name: "synchronization_log",
                newName: "synchronization_log",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "institutio_summary",
                schema: "core",
                newName: "institution_summary",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "institutio_mapping",
                schema: "core",
                newName: "institution_mapping",
                newSchema: "core");

            migrationBuilder.RenameIndex(
                name: "ix_institutio_summary_vendor_id",
                schema: "core",
                table: "institution_summary",
                newName: "ix_institution_summary_vendor_id");

            migrationBuilder.RenameIndex(
                name: "ix_institutio_summary_institution_id",
                schema: "core",
                table: "institution_summary",
                newName: "ix_institution_summary_institution_id");

            migrationBuilder.RenameIndex(
                name: "ix_institutio_mapping_vendor_id",
                schema: "core",
                table: "institution_mapping",
                newName: "ix_institution_mapping_vendor_id");

            migrationBuilder.RenameIndex(
                name: "ix_institutio_mapping_institution_id_vendor_id",
                schema: "core",
                table: "institution_mapping",
                newName: "ix_institution_mapping_institution_id_vendor_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_institution_summary",
                schema: "core",
                table: "institution_summary",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_institution_mapping",
                schema: "core",
                table: "institution_mapping",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_institution_mapping_institution_institution_id",
                schema: "core",
                table: "institution_mapping",
                column: "institution_id",
                principalSchema: "core",
                principalTable: "institution",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_institution_mapping_vendor_vendor_id",
                schema: "core",
                table: "institution_mapping",
                column: "vendor_id",
                principalSchema: "core",
                principalTable: "vendor",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_institution_summary_institution_institution_id",
                schema: "core",
                table: "institution_summary",
                column: "institution_id",
                principalSchema: "core",
                principalTable: "institution",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_institution_summary_vendor_vendor_id",
                schema: "core",
                table: "institution_summary",
                column: "vendor_id",
                principalSchema: "core",
                principalTable: "vendor",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_institution_mapping_institution_institution_id",
                schema: "core",
                table: "institution_mapping");

            migrationBuilder.DropForeignKey(
                name: "fk_institution_mapping_vendor_vendor_id",
                schema: "core",
                table: "institution_mapping");

            migrationBuilder.DropForeignKey(
                name: "fk_institution_summary_institution_institution_id",
                schema: "core",
                table: "institution_summary");

            migrationBuilder.DropForeignKey(
                name: "fk_institution_summary_vendor_vendor_id",
                schema: "core",
                table: "institution_summary");

            migrationBuilder.DropPrimaryKey(
                name: "pk_institution_summary",
                schema: "core",
                table: "institution_summary");

            migrationBuilder.DropPrimaryKey(
                name: "pk_institution_mapping",
                schema: "core",
                table: "institution_mapping");

            migrationBuilder.RenameTable(
                name: "synchronization_log",
                schema: "core",
                newName: "synchronization_log");

            migrationBuilder.RenameTable(
                name: "institution_summary",
                schema: "core",
                newName: "institutio_summary",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "institution_mapping",
                schema: "core",
                newName: "institutio_mapping",
                newSchema: "core");

            migrationBuilder.RenameIndex(
                name: "ix_institution_summary_vendor_id",
                schema: "core",
                table: "institutio_summary",
                newName: "ix_institutio_summary_vendor_id");

            migrationBuilder.RenameIndex(
                name: "ix_institution_summary_institution_id",
                schema: "core",
                table: "institutio_summary",
                newName: "ix_institutio_summary_institution_id");

            migrationBuilder.RenameIndex(
                name: "ix_institution_mapping_vendor_id",
                schema: "core",
                table: "institutio_mapping",
                newName: "ix_institutio_mapping_vendor_id");

            migrationBuilder.RenameIndex(
                name: "ix_institution_mapping_institution_id_vendor_id",
                schema: "core",
                table: "institutio_mapping",
                newName: "ix_institutio_mapping_institution_id_vendor_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_institutio_summary",
                schema: "core",
                table: "institutio_summary",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_institutio_mapping",
                schema: "core",
                table: "institutio_mapping",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_institutio_mapping_institution_institution_id",
                schema: "core",
                table: "institutio_mapping",
                column: "institution_id",
                principalSchema: "core",
                principalTable: "institution",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_institutio_mapping_vendor_vendor_id",
                schema: "core",
                table: "institutio_mapping",
                column: "vendor_id",
                principalSchema: "core",
                principalTable: "vendor",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_institutio_summary_institution_institution_id",
                schema: "core",
                table: "institutio_summary",
                column: "institution_id",
                principalSchema: "core",
                principalTable: "institution",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_institutio_summary_vendor_vendor_id",
                schema: "core",
                table: "institutio_summary",
                column: "vendor_id",
                principalSchema: "core",
                principalTable: "vendor",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
