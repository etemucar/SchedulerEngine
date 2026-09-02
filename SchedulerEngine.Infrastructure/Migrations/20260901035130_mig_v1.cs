using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SchedulerEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mig_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "scheduler_engine");

            migrationBuilder.CreateTable(
                name: "language",
                schema: "scheduler_engine",
                columns: table => new
                {
                    language_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    language_cd = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_language", x => x.language_id);
                });

            migrationBuilder.CreateTable(
                name: "localizable_fields",
                schema: "scheduler_engine",
                columns: table => new
                {
                    localizable_fields_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    entity_field = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_localizable_fields", x => x.localizable_fields_id);
                });

            migrationBuilder.CreateTable(
                name: "localization",
                schema: "scheduler_engine",
                columns: table => new
                {
                    localization_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    entity_id = table.Column<int>(type: "integer", nullable: false),
                    entity_field = table.Column<string>(type: "text", nullable: false),
                    language_cd = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_localization", x => x.localization_id);
                });

            migrationBuilder.CreateTable(
                name: "party",
                schema: "scheduler_engine",
                columns: table => new
                {
                    party_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    party_type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_party", x => x.party_id);
                });

            migrationBuilder.CreateTable(
                name: "service_recurring_job",
                schema: "scheduler_engine",
                columns: table => new
                {
                    service_recurring_job_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    caller_credential_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_name = table.Column<string>(type: "text", nullable: false),
                    recurring_job_id = table.Column<string>(type: "text", nullable: false),
                    hangfire_job_id = table.Column<string>(type: "text", nullable: false),
                    cron_expression = table.Column<string>(type: "text", nullable: false),
                    time_zone_id = table.Column<string>(type: "text", nullable: true),
                    task_name = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    removed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_recurring_job", x => x.service_recurring_job_id);
                });

            migrationBuilder.CreateTable(
                name: "individual",
                schema: "scheduler_engine",
                columns: table => new
                {
                    individual_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    party_id = table.Column<int>(type: "integer", nullable: false),
                    given_name = table.Column<string>(type: "text", nullable: false),
                    family_name = table.Column<string>(type: "text", nullable: false),
                    middle_name = table.Column<string>(type: "text", nullable: true),
                    title = table.Column<string>(type: "text", nullable: true),
                    gender = table.Column<string>(type: "text", nullable: true),
                    nationality = table.Column<string>(type: "text", nullable: true),
                    birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    death_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    place_of_birth = table.Column<string>(type: "text", nullable: true),
                    country_of_birth = table.Column<string>(type: "text", nullable: true),
                    marital_status = table.Column<string>(type: "text", nullable: true),
                    valid_for_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_for_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_individual", x => x.individual_id);
                    table.ForeignKey(
                        name: "FK_individual_party_party_id",
                        column: x => x.party_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "party",
                        principalColumn: "party_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization",
                schema: "scheduler_engine",
                columns: table => new
                {
                    organization_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    party_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    tax_office = table.Column<string>(type: "text", nullable: true),
                    tax_number = table.Column<long>(type: "bigint", nullable: false),
                    identity_number = table.Column<long>(type: "bigint", nullable: false),
                    trade_name = table.Column<string>(type: "text", nullable: true),
                    trade_register_number = table.Column<long>(type: "bigint", nullable: false),
                    mersis_no = table.Column<long>(type: "bigint", nullable: false),
                    valid_for_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_for_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization", x => x.organization_id);
                    table.ForeignKey(
                        name: "FK_organization_party_party_id",
                        column: x => x.party_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "party",
                        principalColumn: "party_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "related_party",
                schema: "scheduler_engine",
                columns: table => new
                {
                    related_party_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    party_id = table.Column<int>(type: "integer", nullable: false),
                    related_to_party_id = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_related_party", x => x.related_party_id);
                    table.ForeignKey(
                        name: "FK_related_party_party_party_id",
                        column: x => x.party_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "party",
                        principalColumn: "party_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_related_party_party_related_to_party_id",
                        column: x => x.related_to_party_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "party",
                        principalColumn: "party_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_language_rel",
                schema: "scheduler_engine",
                columns: table => new
                {
                    organization_language_rel_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organization_id = table.Column<int>(type: "integer", nullable: false),
                    language_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_language_rel", x => x.organization_language_rel_id);
                    table.ForeignKey(
                        name: "FK_organization_language_rel_language_language_id",
                        column: x => x.language_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "language",
                        principalColumn: "language_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_organization_language_rel_organization_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "organization",
                        principalColumn: "organization_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "party_role_type",
                schema: "scheduler_engine",
                columns: table => new
                {
                    party_role_type_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organization_id = table.Column<int>(type: "integer", nullable: true),
                    party_role_type_cd = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_party_role_type", x => x.party_role_type_id);
                    table.ForeignKey(
                        name: "FK_party_role_type_organization_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "organization",
                        principalColumn: "organization_id");
                });

            migrationBuilder.CreateTable(
                name: "party_role",
                schema: "scheduler_engine",
                columns: table => new
                {
                    party_role_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    party_id = table.Column<int>(type: "integer", nullable: false),
                    party_role_type_id = table.Column<int>(type: "integer", nullable: false),
                    valid_for_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_for_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    organization_id = table.Column<int>(type: "integer", nullable: true),
                    party_role_type_id1 = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_party_role", x => x.party_role_id);
                    table.ForeignKey(
                        name: "FK_party_role_organization_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "organization",
                        principalColumn: "organization_id");
                    table.ForeignKey(
                        name: "FK_party_role_party_party_id",
                        column: x => x.party_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "party",
                        principalColumn: "party_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_party_role_party_role_type_party_role_type_id",
                        column: x => x.party_role_type_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "party_role_type",
                        principalColumn: "party_role_type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_party_role_party_role_type_party_role_type_id1",
                        column: x => x.party_role_type_id1,
                        principalSchema: "scheduler_engine",
                        principalTable: "party_role_type",
                        principalColumn: "party_role_type_id");
                });

            migrationBuilder.CreateTable(
                name: "customer",
                schema: "scheduler_engine",
                columns: table => new
                {
                    customer_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    party_role_id = table.Column<int>(type: "integer", nullable: false),
                    customer_number = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer", x => x.customer_id);
                    table.ForeignKey(
                        name: "FK_customer_party_role_party_role_id",
                        column: x => x.party_role_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "party_role",
                        principalColumn: "party_role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "digital_identity",
                schema: "scheduler_engine",
                columns: table => new
                {
                    digital_identity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nickname = table.Column<string>(type: "text", nullable: true),
                    digital_identity_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    party_role_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_digital_identity", x => x.digital_identity_id);
                    table.ForeignKey(
                        name: "FK_digital_identity_party_role_party_role_id",
                        column: x => x.party_role_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "party_role",
                        principalColumn: "party_role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "party_role_account",
                schema: "scheduler_engine",
                columns: table => new
                {
                    party_role_account_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    party_role_id = table.Column<int>(type: "integer", nullable: false),
                    currency_code = table.Column<string>(type: "text", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_party_role_account", x => x.party_role_account_id);
                    table.ForeignKey(
                        name: "FK_party_role_account_customer_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "customer",
                        principalColumn: "customer_id");
                    table.ForeignKey(
                        name: "FK_party_role_account_party_role_party_role_id",
                        column: x => x.party_role_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "party_role",
                        principalColumn: "party_role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "application_user",
                schema: "scheduler_engine",
                columns: table => new
                {
                    application_user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    external_user_id = table.Column<string>(type: "text", nullable: true),
                    language_id = table.Column<int>(type: "integer", nullable: false),
                    digital_identity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_user", x => x.application_user_id);
                    table.ForeignKey(
                        name: "FK_application_user_digital_identity_digital_identity_id",
                        column: x => x.digital_identity_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "digital_identity",
                        principalColumn: "digital_identity_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_application_user_language_language_id",
                        column: x => x.language_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "language",
                        principalColumn: "language_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "credential",
                schema: "scheduler_engine",
                columns: table => new
                {
                    credential_id = table.Column<Guid>(type: "uuid", nullable: false),
                    credential_type = table.Column<int>(type: "integer", nullable: false),
                    trust_level = table.Column<int>(type: "integer", nullable: true),
                    digital_identity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credential", x => x.credential_id);
                    table.ForeignKey(
                        name: "FK_credential_digital_identity_digital_identity_id",
                        column: x => x.digital_identity_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "digital_identity",
                        principalColumn: "digital_identity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_token",
                schema: "scheduler_engine",
                columns: table => new
                {
                    refresh_token_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_ip = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    application_user_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_token", x => x.refresh_token_id);
                    table.ForeignKey(
                        name: "FK_refresh_token_application_user_application_user_id",
                        column: x => x.application_user_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "application_user",
                        principalColumn: "application_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contact_medium",
                schema: "scheduler_engine",
                columns: table => new
                {
                    contact_medium_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    party_id = table.Column<int>(type: "integer", nullable: false),
                    credential_id = table.Column<Guid>(type: "uuid", nullable: true),
                    medium_type = table.Column<int>(type: "integer", nullable: false),
                    is_preferred = table.Column<bool>(type: "boolean", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    url = table.Column<string>(type: "text", nullable: true),
                    address_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact_medium", x => x.contact_medium_id);
                    table.ForeignKey(
                        name: "FK_contact_medium_credential_credential_id",
                        column: x => x.credential_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "credential",
                        principalColumn: "credential_id");
                    table.ForeignKey(
                        name: "FK_contact_medium_party_party_id",
                        column: x => x.party_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "party",
                        principalColumn: "party_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "credential_characteristic",
                schema: "scheduler_engine",
                columns: table => new
                {
                    credential_characteristic_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    credential_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credential_characteristic", x => x.credential_characteristic_id);
                    table.ForeignKey(
                        name: "FK_credential_characteristic_credential_credential_id",
                        column: x => x.credential_id,
                        principalSchema: "scheduler_engine",
                        principalTable: "credential",
                        principalColumn: "credential_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "scheduler_engine",
                table: "language",
                columns: new[] { "language_id", "create_date", "created_by", "is_deleted", "language_cd", "name", "status", "update_date", "updated_by" },
                values: new object[,]
                {
                    { 1, null, null, 0, "tr", "Türkçe", 0, null, null },
                    { 2, null, null, 0, "en", "English", 0, null, null },
                    { 3, null, null, 0, "ru", "Русский", 0, null, null }
                });

            migrationBuilder.InsertData(
                schema: "scheduler_engine",
                table: "localizable_fields",
                columns: new[] { "localizable_fields_id", "create_date", "created_by", "entity_field", "entity_type", "is_deleted", "status", "update_date", "updated_by" },
                values: new object[,]
                {
                    { 1, null, null, "Name", "Status", 0, 0, null, null },
                    { 2, null, null, "Description", "Status", 0, 0, null, null }
                });

            migrationBuilder.InsertData(
                schema: "scheduler_engine",
                table: "party_role_type",
                columns: new[] { "party_role_type_id", "create_date", "created_by", "is_deleted", "name", "organization_id", "party_role_type_cd", "status", "update_date", "updated_by" },
                values: new object[,]
                {
                    { 1, null, null, 0, "Site Yöneticis", null, "SITE_ADMIN", 0, null, null },
                    { 2, null, null, 0, "Uygulama Kullanıcısı", null, "USER", 0, null, null },
                    { 3, null, null, 0, "Müşteri", null, "CUSTOMER", 0, null, null },
                    { 4, null, null, 0, "Fatura Hesabı", null, "BILL_ACCOUNT", 0, null, null },
                    { 5, null, null, 0, "Dış servis hesabı", null, "EXTERNAL_SERVICE", 0, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_application_user_digital_identity_id",
                schema: "scheduler_engine",
                table: "application_user",
                column: "digital_identity_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_application_user_language_id",
                schema: "scheduler_engine",
                table: "application_user",
                column: "language_id");

            migrationBuilder.CreateIndex(
                name: "IX_contact_medium_credential_id",
                schema: "scheduler_engine",
                table: "contact_medium",
                column: "credential_id");

            migrationBuilder.CreateIndex(
                name: "IX_contact_medium_party_id",
                schema: "scheduler_engine",
                table: "contact_medium",
                column: "party_id");

            migrationBuilder.CreateIndex(
                name: "IX_credential_digital_identity_id",
                schema: "scheduler_engine",
                table: "credential",
                column: "digital_identity_id");

            migrationBuilder.CreateIndex(
                name: "IX_credential_characteristic_credential_id",
                schema: "scheduler_engine",
                table: "credential_characteristic",
                column: "credential_id");

            migrationBuilder.CreateIndex(
                name: "IX_customer_party_role_id",
                schema: "scheduler_engine",
                table: "customer",
                column: "party_role_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_digital_identity_party_role_id",
                schema: "scheduler_engine",
                table: "digital_identity",
                column: "party_role_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_individual_party_id",
                schema: "scheduler_engine",
                table: "individual",
                column: "party_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_language_language_cd",
                schema: "scheduler_engine",
                table: "language",
                column: "language_cd",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_party_id",
                schema: "scheduler_engine",
                table: "organization",
                column: "party_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_language_rel_language_id",
                schema: "scheduler_engine",
                table: "organization_language_rel",
                column: "language_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_language_rel_organization_id_language_id",
                schema: "scheduler_engine",
                table: "organization_language_rel",
                columns: new[] { "organization_id", "language_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_party_role_organization_id",
                schema: "scheduler_engine",
                table: "party_role",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_party_role_party_id",
                schema: "scheduler_engine",
                table: "party_role",
                column: "party_id");

            migrationBuilder.CreateIndex(
                name: "IX_party_role_party_role_type_id",
                schema: "scheduler_engine",
                table: "party_role",
                column: "party_role_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_party_role_party_role_type_id1",
                schema: "scheduler_engine",
                table: "party_role",
                column: "party_role_type_id1");

            migrationBuilder.CreateIndex(
                name: "IX_party_role_account_customer_id",
                schema: "scheduler_engine",
                table: "party_role_account",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_party_role_account_party_role_id",
                schema: "scheduler_engine",
                table: "party_role_account",
                column: "party_role_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_party_role_type_organization_id",
                schema: "scheduler_engine",
                table: "party_role_type",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_application_user_id",
                schema: "scheduler_engine",
                table: "refresh_token",
                column: "application_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_token",
                schema: "scheduler_engine",
                table: "refresh_token",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_related_party_party_id",
                schema: "scheduler_engine",
                table: "related_party",
                column: "party_id");

            migrationBuilder.CreateIndex(
                name: "IX_related_party_related_to_party_id",
                schema: "scheduler_engine",
                table: "related_party",
                column: "related_to_party_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contact_medium",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "credential_characteristic",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "individual",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "localizable_fields",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "localization",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "organization_language_rel",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "party_role_account",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "refresh_token",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "related_party",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "service_recurring_job",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "credential",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "customer",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "application_user",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "digital_identity",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "language",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "party_role",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "party_role_type",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "organization",
                schema: "scheduler_engine");

            migrationBuilder.DropTable(
                name: "party",
                schema: "scheduler_engine");
        }
    }
}
