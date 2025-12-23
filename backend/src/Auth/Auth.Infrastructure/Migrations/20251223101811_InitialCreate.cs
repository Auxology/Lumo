using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "login_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_key = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    otp_token_hash = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    magic_link_token_hash = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    consumed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    fingerprint_computed_hash = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    fingerprint_ip_address = table.Column<string>(type: "varchar", maxLength: 45, nullable: false),
                    fingerprint_language = table.Column<string>(type: "varchar", maxLength: 16, nullable: false),
                    fingerprint_normalized_browser = table.Column<string>(type: "varchar", maxLength: 128, nullable: false),
                    fingerprint_normalized_os = table.Column<string>(type: "varchar", maxLength: 128, nullable: false),
                    fingerprint_timezone = table.Column<string>(type: "varchar", maxLength: 64, nullable: false),
                    fingerprint_user_agent = table.Column<string>(type: "varchar", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_login_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recovery_key_chains",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    last_rotated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recovery_key_chains", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    refresh_token_key = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    refresh_token_hash = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    last_refreshed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    revoke_reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    fingerprint_computed_hash = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    fingerprint_ip_address = table.Column<string>(type: "varchar", maxLength: 45, nullable: false),
                    fingerprint_language = table.Column<string>(type: "varchar", maxLength: 16, nullable: false),
                    fingerprint_normalized_browser = table.Column<string>(type: "varchar", maxLength: 128, nullable: false),
                    fingerprint_normalized_os = table.Column<string>(type: "varchar", maxLength: 128, nullable: false),
                    fingerprint_timezone = table.Column<string>(type: "varchar", maxLength: 64, nullable: false),
                    fingerprint_user_agent = table.Column<string>(type: "varchar", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "varchar", maxLength: 256, nullable: false),
                    email_address = table.Column<string>(type: "varchar", nullable: false),
                    avatar_key = table.Column<string>(type: "varchar", nullable: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recovery_keys",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    recovery_key_chain_id = table.Column<Guid>(type: "uuid", nullable: false),
                    identifier = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    verifier_hash = table.Column<string>(type: "varchar", maxLength: 512, nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    fingerprint_computed_hash = table.Column<string>(type: "varchar", maxLength: 512, nullable: true),
                    fingerprint_ip_address = table.Column<string>(type: "varchar", maxLength: 45, nullable: true),
                    fingerprint_language = table.Column<string>(type: "varchar", maxLength: 16, nullable: true),
                    fingerprint_normalized_browser = table.Column<string>(type: "varchar", maxLength: 128, nullable: true),
                    fingerprint_normalized_os = table.Column<string>(type: "varchar", maxLength: 128, nullable: true),
                    fingerprint_timezone = table.Column<string>(type: "varchar", maxLength: 64, nullable: true),
                    fingerprint_user_agent = table.Column<string>(type: "varchar", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recovery_keys", x => x.id);
                    table.ForeignKey(
                        name: "fk_recovery_keys_recovery_key_chains_recovery_key_chain_id",
                        column: x => x.recovery_key_chain_id,
                        principalTable: "recovery_key_chains",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_login_requests_consumed_at",
                table: "login_requests",
                column: "consumed_at");

            migrationBuilder.CreateIndex(
                name: "ix_login_requests_expires_at",
                table: "login_requests",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_login_requests_token_key",
                table: "login_requests",
                column: "token_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_login_requests_token_key_consumed_at_expires_at",
                table: "login_requests",
                columns: new[] { "token_key", "consumed_at", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_login_requests_user_id",
                table: "login_requests",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_recovery_key_chains_created_at",
                table: "recovery_key_chains",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_recovery_key_chains_user_id",
                table: "recovery_key_chains",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_recovery_keys_identifier",
                table: "recovery_keys",
                column: "identifier");

            migrationBuilder.CreateIndex(
                name: "ix_recovery_keys_identifier_is_used",
                table: "recovery_keys",
                columns: new[] { "identifier", "is_used" });

            migrationBuilder.CreateIndex(
                name: "ix_recovery_keys_is_used",
                table: "recovery_keys",
                column: "is_used");

            migrationBuilder.CreateIndex(
                name: "ix_recovery_keys_recovery_key_chain_id",
                table: "recovery_keys",
                column: "recovery_key_chain_id");

            migrationBuilder.CreateIndex(
                name: "ix_recovery_keys_recovery_key_chain_id_is_used",
                table: "recovery_keys",
                columns: new[] { "recovery_key_chain_id", "is_used" });

            migrationBuilder.CreateIndex(
                name: "ix_sessions_expires_at",
                table: "sessions",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_sessions_expires_at_revoked_at",
                table: "sessions",
                columns: new[] { "expires_at", "revoked_at" });

            migrationBuilder.CreateIndex(
                name: "ix_sessions_refresh_token_key",
                table: "sessions",
                column: "refresh_token_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sessions_revoked_at",
                table: "sessions",
                column: "revoked_at");

            migrationBuilder.CreateIndex(
                name: "ix_sessions_user_id",
                table: "sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessions_user_id_revoked_at",
                table: "sessions",
                columns: new[] { "user_id", "revoked_at" });

            migrationBuilder.CreateIndex(
                name: "ix_users_created_at",
                table: "users",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_users_email_address",
                table: "users",
                column: "email_address",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_is_verified",
                table: "users",
                column: "is_verified");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "login_requests");

            migrationBuilder.DropTable(
                name: "recovery_keys");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "recovery_key_chains");
        }
    }
}
