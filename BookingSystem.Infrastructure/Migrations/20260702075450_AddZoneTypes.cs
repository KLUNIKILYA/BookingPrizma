using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddZoneTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Booking_ZoneType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking_ZoneType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Booking_ZoneAssignment",
                columns: table => new
                {
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    ZoneTypeId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking_ZoneAssignment", x => x.ZoneId);
                    table.ForeignKey(
                        name: "FK_Booking_ZoneAssignment_Booking_ZoneType_ZoneTypeId",
                        column: x => x.ZoneTypeId,
                        principalTable: "Booking_ZoneType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_ZoneAssignment_ZoneTypeId",
                table: "Booking_ZoneAssignment",
                column: "ZoneTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Booking_ZoneAssignment");

            migrationBuilder.DropTable(
                name: "Booking_ZoneType");
        }
    }
}
