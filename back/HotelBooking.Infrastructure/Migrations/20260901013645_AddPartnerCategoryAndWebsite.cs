using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerCategoryAndWebsite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Partners",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Other");

            migrationBuilder.AddColumn<int>(
                name: "ClickCount",
                table: "Partners",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Partners",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "ClickCount",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Partners");
        }
    }
}
