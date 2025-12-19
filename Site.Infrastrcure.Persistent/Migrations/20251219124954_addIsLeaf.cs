using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Site.Infrastrcure.Persistent.Migrations
{
    /// <inheritdoc />
    public partial class addIsLeaf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLeaf",
                table: "Sites",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLeaf",
                table: "Sites");
        }
    }
}
