using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booking.Infrastrcure.Persistent.Migrations
{
    /// <inheritdoc />
    public partial class remove_name_index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sites_NameAr",
                table: "Sites");

            migrationBuilder.DropIndex(
                name: "IX_Sites_NameEn",
                table: "Sites");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sites_NameAr",
                table: "Sites",
                column: "NameAr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sites_NameEn",
                table: "Sites",
                column: "NameEn",
                unique: true);
        }
    }
}
