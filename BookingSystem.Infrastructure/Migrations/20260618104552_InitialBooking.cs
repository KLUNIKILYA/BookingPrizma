using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Booking_Resource",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitorId = table.Column<int>(type: "int", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking_Resource", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Booking_ServiceSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    BreakMinutes = table.Column<int>(type: "int", nullable: false),
                    ColorOverride = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    IsBookable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking_ServiceSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Booking_Booking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResourceId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ClientVisitorId = table.Column<int>(type: "int", nullable: true),
                    Label = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking_Booking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Booking_Booking_Booking_Resource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Booking_Resource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Booking_BookingService",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DurationSnapshot = table.Column<int>(type: "int", nullable: false),
                    BreakSnapshot = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsDone = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking_BookingService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Booking_BookingService_Booking_Booking_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Booking_Booking",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Booking_ResourceId_TimeFrom",
                table: "Booking_Booking",
                columns: new[] { "ResourceId", "TimeFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Booking_TimeFrom",
                table: "Booking_Booking",
                column: "TimeFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_BookingService_BookingId",
                table: "Booking_BookingService",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Resource_VisitorId",
                table: "Booking_Resource",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_ServiceSetting_ServiceId",
                table: "Booking_ServiceSetting",
                column: "ServiceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Booking_BookingService");

            migrationBuilder.DropTable(
                name: "Booking_ServiceSetting");

            migrationBuilder.DropTable(
                name: "Booking_Booking");

            migrationBuilder.DropTable(
                name: "Booking_Resource");
        }
    }
}
