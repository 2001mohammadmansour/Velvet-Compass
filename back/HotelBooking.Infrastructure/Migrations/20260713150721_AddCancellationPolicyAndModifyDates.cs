using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCancellationPolicyAndModifyDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationFeeType",
                table: "Hotels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Percentage");

            migrationBuilder.AddColumn<decimal>(
                name: "CancellationFeeValue",
                table: "Hotels",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 20m);

            migrationBuilder.AddColumn<int>(
                name: "FreeCancellationDaysBefore",
                table: "Hotels",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<bool>(
                name: "FreeCancellationEnabled",
                table: "Hotels",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ModificationFee",
                table: "Bookings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationFeeType",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "CancellationFeeValue",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "FreeCancellationDaysBefore",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "FreeCancellationEnabled",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ModificationFee",
                table: "Bookings");
        }
    }
}
