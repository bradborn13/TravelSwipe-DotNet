using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelSwipe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_country_name_clean",
                table: "Country",
                column: "name_clean",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_city_name_clean",
                table: "City",
                column: "name_clean",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_country_name_clean",
                table: "Country");

            migrationBuilder.DropIndex(
                name: "ix_city_name_clean",
                table: "City");
        }
    }
}
