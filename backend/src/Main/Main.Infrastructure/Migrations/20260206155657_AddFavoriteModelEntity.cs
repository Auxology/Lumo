using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoriteModelEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "favorite_model",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(30)", nullable: false),
                    preference_id = table.Column<string>(type: "varchar(30)", nullable: false),
                    model_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favorite_model", x => x.id);
                    table.ForeignKey(
                        name: "fk_favorite_model_preferences_preference_id",
                        column: x => x.preference_id,
                        principalTable: "preferences",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_favorite_model_preference_id",
                table: "favorite_model",
                column: "preference_id");

            migrationBuilder.CreateIndex(
                name: "ix_favorite_model_preference_id_model_id",
                table: "favorite_model",
                columns: new[] { "preference_id", "model_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "favorite_model");
        }
    }
}
