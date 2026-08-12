using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverIdToTruck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trucks_Plate",
                table: "Trucks");

            migrationBuilder.AddColumn<int>(
                name: "DriverId",
                table: "Trucks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_DriverId_Plate",
                table: "Trucks",
                columns: new[] { "DriverId", "Plate" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Truck_Hodometer_NonNegative",
                table: "Trucks",
                sql: "\"Hodometer\" >= 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_Drivers_DriverId",
                table: "Trucks",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_Drivers_DriverId",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_DriverId_Plate",
                table: "Trucks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Truck_Hodometer_NonNegative",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "Trucks");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_Plate",
                table: "Trucks",
                column: "Plate",
                unique: true);
        }
    }
}
