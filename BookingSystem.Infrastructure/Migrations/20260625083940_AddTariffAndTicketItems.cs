using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTariffAndTicketItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTicket",
                table: "Booking_ResService",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TariffName",
                table: "Booking_ResExtra",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TariffPrice",
                table: "Booking_ResExtra",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TariffTicketId",
                table: "Booking_ResExtra",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTicket",
                table: "Booking_ResService");

            migrationBuilder.DropColumn(
                name: "TariffName",
                table: "Booking_ResExtra");

            migrationBuilder.DropColumn(
                name: "TariffPrice",
                table: "Booking_ResExtra");

            migrationBuilder.DropColumn(
                name: "TariffTicketId",
                table: "Booking_ResExtra");
        }
    }
}
