using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballPredictionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueGoogleIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_GoogleId",
                table: "Users",
                column: "GoogleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_GoogleId",
                table: "Users");
        }
    }
}
