using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddStartOdometerToRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "StartOdometer",
                table: "Routes",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Route_StartOdometer_NonNegative",
                table: "Routes",
                sql: "\"StartOdometer\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Route_StartOdometer_NonNegative",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "StartOdometer",
                table: "Routes");
        }
    }
}
