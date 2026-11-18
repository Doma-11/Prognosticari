using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballPredictionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonExternalApiId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExternalApiId",
                table: "Seasons",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_ExternalApiId",
                table: "Seasons",
                column: "ExternalApiId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Seasons_ExternalApiId",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "ExternalApiId",
                table: "Seasons");
        }
    }
}
