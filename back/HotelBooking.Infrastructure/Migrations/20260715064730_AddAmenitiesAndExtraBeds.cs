using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAmenitiesAndExtraBeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowExtraBed",
                table: "RoomTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtraBedPriceForOneBed",
                table: "RoomTypes",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtraBedPriceForTwoBeds",
                table: "RoomTypes",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ExtraBedPriceType",
                table: "RoomTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Percentage");

            migrationBuilder.AddColumn<int>(
                name: "MaxExtraBeds",
                table: "RoomTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExtraBedCount",
                table: "BookingItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtraBedFee",
                table: "BookingItems",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Amenities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Scope = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HotelAmenities",
                columns: table => new
                {
                    HotelId = table.Column<long>(type: "bigint", nullable: false),
                    AmenityId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelAmenities", x => new { x.HotelId, x.AmenityId });
                    table.ForeignKey(
                        name: "FK_HotelAmenities_Amenities_AmenityId",
                        column: x => x.AmenityId,
                        principalTable: "Amenities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HotelAmenities_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomTypeAmenities",
                columns: table => new
                {
                    RoomTypeId = table.Column<long>(type: "bigint", nullable: false),
                    AmenityId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomTypeAmenities", x => new { x.RoomTypeId, x.AmenityId });
                    table.ForeignKey(
                        name: "FK_RoomTypeAmenities_Amenities_AmenityId",
                        column: x => x.AmenityId,
                        principalTable: "Amenities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoomTypeAmenities_RoomTypes_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalTable: "RoomTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HotelAmenities_AmenityId",
                table: "HotelAmenities",
                column: "AmenityId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTypeAmenities_AmenityId",
                table: "RoomTypeAmenities",
                column: "AmenityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HotelAmenities");

            migrationBuilder.DropTable(
                name: "RoomTypeAmenities");

            migrationBuilder.DropTable(
                name: "Amenities");

            migrationBuilder.DropColumn(
                name: "AllowExtraBed",
                table: "RoomTypes");

            migrationBuilder.DropColumn(
                name: "ExtraBedPriceForOneBed",
                table: "RoomTypes");

            migrationBuilder.DropColumn(
                name: "ExtraBedPriceForTwoBeds",
                table: "RoomTypes");

            migrationBuilder.DropColumn(
                name: "ExtraBedPriceType",
                table: "RoomTypes");

            migrationBuilder.DropColumn(
                name: "MaxExtraBeds",
                table: "RoomTypes");

            migrationBuilder.DropColumn(
                name: "ExtraBedCount",
                table: "BookingItems");

            migrationBuilder.DropColumn(
                name: "ExtraBedFee",
                table: "BookingItems");
        }
    }
}
