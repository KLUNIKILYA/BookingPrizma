using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCelebrantAndPrepayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CelebrantBirthDate",
                table: "Booking_ResExtra",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CelebrantName",
                table: "Booking_ResExtra",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrepaid",
                table: "Booking_ResExtra",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepaidAmount",
                table: "Booking_ResExtra",
                type: "numeric(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CelebrantBirthDate",
                table: "Booking_ResExtra");

            migrationBuilder.DropColumn(
                name: "CelebrantName",
                table: "Booking_ResExtra");

            migrationBuilder.DropColumn(
                name: "IsPrepaid",
                table: "Booking_ResExtra");

            migrationBuilder.DropColumn(
                name: "PrepaidAmount",
                table: "Booking_ResExtra");
        }
    }
}
