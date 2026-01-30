using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Main.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "memories",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(30)", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    category = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    embedding = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_memories", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_memories_embedding",
                table: "memories",
                column: "embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_memories_user_id",
                table: "memories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_memories_user_id_created_at",
                table: "memories",
                columns: new[] { "user_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "memories");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
