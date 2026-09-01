using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSettlementModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing bookings predate the online/cash split. Treat them as pay-on-arrival,
            // matching the old rule that a booking with no payment record was cash.
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Bookings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "CashOnArrival");

            // A paid/refunded Payment row means the guest paid the platform up front — that's an
            // online booking, not cash.
            migrationBuilder.Sql(@"
                UPDATE b SET b.PaymentMethod = 'Online'
                FROM Bookings b
                INNER JOIN Payments p ON p.BookingId = b.Id
                WHERE p.Status IN ('Paid', 'Refunded');");

            migrationBuilder.AddColumn<DateTime>(
                name: "SettledAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SettlementId",
                table: "Bookings",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Settlements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HotelId = table.Column<long>(type: "bigint", nullable: false),
                    PeriodLabel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OwnerCredit = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    PlatformCommission = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    BookingCount = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Settlements_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SettlementId",
                table: "Bookings",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_HotelId",
                table: "Settlements",
                column: "HotelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Settlements_SettlementId",
                table: "Bookings",
                column: "SettlementId",
                principalTable: "Settlements",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Settlements_SettlementId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "Settlements");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_SettlementId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SettledAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SettlementId",
                table: "Bookings");
        }
    }
}
