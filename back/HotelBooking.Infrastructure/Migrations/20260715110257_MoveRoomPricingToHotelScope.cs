using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveRoomPricingToHotelScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OccupancyPriceTiers_RoomTypes_RoomTypeId",
                table: "OccupancyPriceTiers");

            migrationBuilder.DropForeignKey(
                name: "FK_SeasonalPriceRules_RoomTypes_RoomTypeId",
                table: "SeasonalPriceRules");

            migrationBuilder.RenameColumn(
                name: "RoomTypeId",
                table: "SeasonalPriceRules",
                newName: "HotelId");

            migrationBuilder.RenameIndex(
                name: "IX_SeasonalPriceRules_RoomTypeId",
                table: "SeasonalPriceRules",
                newName: "IX_SeasonalPriceRules_HotelId");

            migrationBuilder.RenameColumn(
                name: "RoomTypeId",
                table: "OccupancyPriceTiers",
                newName: "HotelId");

            migrationBuilder.RenameIndex(
                name: "IX_OccupancyPriceTiers_RoomTypeId",
                table: "OccupancyPriceTiers",
                newName: "IX_OccupancyPriceTiers_HotelId");

            // CHANGED BY AI (2026-07-15): please review. The renames above only change the COLUMN
            // NAME — the values are still the old RoomTypeId values, not real HotelIds. Fix them up
            // via the still-intact RoomTypes table before adding the new Hotels FK, then dedupe:
            // every room type in a hotel used to get its own copy of the same default rule, so
            // moving to hotel scope would otherwise leave 2-3 duplicate rows per hotel.
            migrationBuilder.Sql(@"
                UPDATE spr
                SET spr.HotelId = rt.HotelId
                FROM SeasonalPriceRules spr
                INNER JOIN RoomTypes rt ON rt.Id = spr.HotelId;

                UPDATE opt
                SET opt.HotelId = rt.HotelId
                FROM OccupancyPriceTiers opt
                INNER JOIN RoomTypes rt ON rt.Id = opt.HotelId;

                ;WITH dupes AS (
                    SELECT Id, ROW_NUMBER() OVER (PARTITION BY HotelId, Name ORDER BY Id) AS rn
                    FROM SeasonalPriceRules
                )
                DELETE FROM SeasonalPriceRules WHERE Id IN (SELECT Id FROM dupes WHERE rn > 1);

                ;WITH dupes AS (
                    SELECT Id, ROW_NUMBER() OVER (PARTITION BY HotelId, MinOccupancyPercent ORDER BY Id) AS rn
                    FROM OccupancyPriceTiers
                )
                DELETE FROM OccupancyPriceTiers WHERE Id IN (SELECT Id FROM dupes WHERE rn > 1);
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_OccupancyPriceTiers_Hotels_HotelId",
                table: "OccupancyPriceTiers",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonalPriceRules_Hotels_HotelId",
                table: "SeasonalPriceRules",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OccupancyPriceTiers_Hotels_HotelId",
                table: "OccupancyPriceTiers");

            migrationBuilder.DropForeignKey(
                name: "FK_SeasonalPriceRules_Hotels_HotelId",
                table: "SeasonalPriceRules");

            migrationBuilder.RenameColumn(
                name: "HotelId",
                table: "SeasonalPriceRules",
                newName: "RoomTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_SeasonalPriceRules_HotelId",
                table: "SeasonalPriceRules",
                newName: "IX_SeasonalPriceRules_RoomTypeId");

            migrationBuilder.RenameColumn(
                name: "HotelId",
                table: "OccupancyPriceTiers",
                newName: "RoomTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_OccupancyPriceTiers_HotelId",
                table: "OccupancyPriceTiers",
                newName: "IX_OccupancyPriceTiers_RoomTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_OccupancyPriceTiers_RoomTypes_RoomTypeId",
                table: "OccupancyPriceTiers",
                column: "RoomTypeId",
                principalTable: "RoomTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonalPriceRules_RoomTypes_RoomTypeId",
                table: "SeasonalPriceRules",
                column: "RoomTypeId",
                principalTable: "RoomTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
