using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LinkPara.Identity.Infrastructure.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostgreInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "agreement_document",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_version = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_document", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "device_info",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    device_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    device_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    registration_token = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    manufacturer = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    model = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    operating_system = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    operating_system_version = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    screen_resolution = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    app_version = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    app_build_number = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    camera = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_info", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permission",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    claim_value = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    module = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    operation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    normalized_claim_value = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    description = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    display = table.Column<bool>(type: "boolean", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permission", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    role_scope = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "security_question",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    question = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_security_question", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fathers_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    nation_country_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    serial_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    identity_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    birth_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    user_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    internal_user_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source_channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    profession = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    password_modified_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status_change_reason = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    login_last_activity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agreement_document_version",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agreement_document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    version = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    is_latest = table.Column<bool>(type: "boolean", nullable: false),
                    is_force_update = table.Column<bool>(type: "boolean", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_document_version", x => x.id);
                    table.ForeignKey(
                        name: "fk_agreement_document_version_agreement_document_agreement_doc",
                        column: x => x.agreement_document_id,
                        principalSchema: "core",
                        principalTable: "agreement_document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_device_info",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_main_device = table.Column<bool>(type: "boolean", nullable: false),
                    device_info_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_device_info", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_device_info_device_info_device_info_id",
                        column: x => x.device_info_id,
                        principalSchema: "core",
                        principalTable: "device_info",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_claim",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_claim", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_claim_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "core",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_address",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    country_id = table.Column<int>(type: "integer", nullable: false),
                    city_id = table.Column<int>(type: "integer", nullable: false),
                    district_id = table.Column<int>(type: "integer", nullable: false),
                    neighbourhood = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    street = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    full_address = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_address", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_address_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "core",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_claim",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    display_name = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_claim", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_claim_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "core",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_login",
                schema: "core",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_login", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_user_login_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "core",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_login_last_activity",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_succeeded_login = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_locked_login = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_failed_login = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    login_result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_login_last_activity", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_login_last_activity_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "core",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_password_history",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_password_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_password_history_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "core",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_picture",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bytes = table.Column<byte[]>(type: "bytea", nullable: true),
                    content_type = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_picture", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_picture_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "core",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                schema: "core",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_role_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "core",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_role_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "core",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_security_answer",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", maxLength: 450, nullable: false),
                    security_question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    answer_hash = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_security_answer", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_security_answer_security_question_security_question_id",
                        column: x => x.security_question_id,
                        principalSchema: "core",
                        principalTable: "security_question",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_security_answer_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "core",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_token",
                schema: "core",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_token", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_user_token_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "core",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_agreement_document",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agreement_document_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    record_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_agreement_document", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_agreement_document_agreement_document_version_agreemen",
                        column: x => x.agreement_document_version_id,
                        principalSchema: "core",
                        principalTable: "agreement_document_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_agreement_document_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "core",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agreement_document_record_status",
                schema: "core",
                table: "agreement_document",
                column: "record_status");

            migrationBuilder.CreateIndex(
                name: "ix_agreement_document_version_agreement_document_id",
                schema: "core",
                table: "agreement_document_version",
                column: "agreement_document_id");

            migrationBuilder.CreateIndex(
                name: "ix_device_info_device_id",
                schema: "core",
                table: "device_info",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_permission_claim_value",
                schema: "core",
                table: "permission",
                column: "claim_value",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_name",
                schema: "core",
                table: "role",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "core",
                table: "role",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_claim_role_id",
                schema: "core",
                table: "role_claim",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "core",
                table: "user",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "ix_user_email",
                schema: "core",
                table: "user",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_user_phone_number",
                schema: "core",
                table: "user",
                column: "phone_number");

            migrationBuilder.CreateIndex(
                name: "ix_user_user_name",
                schema: "core",
                table: "user",
                column: "user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_user_status",
                schema: "core",
                table: "user",
                column: "user_status");

            migrationBuilder.CreateIndex(
                name: "ix_user_user_type",
                schema: "core",
                table: "user",
                column: "user_type");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "core",
                table: "user",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_address_user_id",
                schema: "core",
                table: "user_address",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_agreement_document_agreement_document_version_id",
                schema: "core",
                table: "user_agreement_document",
                column: "agreement_document_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_agreement_document_user_id",
                schema: "core",
                table: "user_agreement_document",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_claim_user_id",
                schema: "core",
                table: "user_claim",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_device_info_device_info_id",
                schema: "core",
                table: "user_device_info",
                column: "device_info_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_login_user_id",
                schema: "core",
                table: "user_login",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_login_last_activity_user_id",
                schema: "core",
                table: "user_login_last_activity",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_password_history_user_id",
                schema: "core",
                table: "user_password_history",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_picture_user_id",
                schema: "core",
                table: "user_picture",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_role_id",
                schema: "core",
                table: "user_role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_security_answer_security_question_id",
                schema: "core",
                table: "user_security_answer",
                column: "security_question_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_security_answer_user_id",
                schema: "core",
                table: "user_security_answer",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "permission",
                schema: "core");

            migrationBuilder.DropTable(
                name: "role_claim",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_address",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_agreement_document",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_claim",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_device_info",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_login",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_login_last_activity",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_password_history",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_picture",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_role",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_security_answer",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_token",
                schema: "core");

            migrationBuilder.DropTable(
                name: "agreement_document_version",
                schema: "core");

            migrationBuilder.DropTable(
                name: "device_info",
                schema: "core");

            migrationBuilder.DropTable(
                name: "role",
                schema: "core");

            migrationBuilder.DropTable(
                name: "security_question",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user",
                schema: "core");

            migrationBuilder.DropTable(
                name: "agreement_document",
                schema: "core");
        }
    }
}
