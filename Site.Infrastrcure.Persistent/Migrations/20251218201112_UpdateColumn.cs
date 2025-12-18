using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Site.Infrastrcure.Persistent.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Sites",
                newName: "PricePerHour");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PricePerHour",
                table: "Sites",
                newName: "Price");
        }
    }
}
