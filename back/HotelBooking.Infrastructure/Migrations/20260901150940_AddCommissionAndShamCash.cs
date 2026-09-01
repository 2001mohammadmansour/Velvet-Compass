using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommissionAndShamCash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShamCashQrUrl",
                table: "Hotels",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShamCashWallet",
                table: "Hotels",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionAmount",
                table: "Bookings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CommissionClaimedAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CommissionPaidAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShamCashQrUrl",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "ShamCashWallet",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "CommissionAmount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CommissionClaimedAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CommissionPaidAt",
                table: "Bookings");
        }
    }
}
