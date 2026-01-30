using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Main.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shared_chats",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(30)", nullable: false),
                    source_chat_id = table.Column<string>(type: "varchar(30)", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    model_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    snapshot_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_chats", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shared_chat_messages",
                columns: table => new
                {
                    sequence_number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shared_chat_id = table.Column<string>(type: "varchar(30)", nullable: false),
                    message_role = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    message_content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_chat_messages", x => new { x.shared_chat_id, x.sequence_number });
                    table.ForeignKey(
                        name: "fk_shared_chat_messages_shared_chats_shared_chat_id",
                        column: x => x.shared_chat_id,
                        principalTable: "shared_chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shared_chats_owner_id",
                table: "shared_chats",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_chats_source_chat_id",
                table: "shared_chats",
                column: "source_chat_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shared_chat_messages");

            migrationBuilder.DropTable(
                name: "shared_chats");
        }
    }
}
