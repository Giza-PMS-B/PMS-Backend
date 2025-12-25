using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoice.Infrastrcure.Persistent.Migrations
{
    /// <inheritdoc />
    public partial class updateHtmlDoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HtmlDocumentPath",
                table: "Invoices",
                newName: "HtmlDocument");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HtmlDocument",
                table: "Invoices",
                newName: "HtmlDocumentPath");
        }
    }
}
