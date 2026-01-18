using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Main.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "model_name",
                table: "chats",
                newName: "model_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "model_id",
                table: "chats",
                newName: "model_name");
        }
    }
}
