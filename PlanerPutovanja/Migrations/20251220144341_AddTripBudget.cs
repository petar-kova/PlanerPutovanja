using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanerPutovanja.Migrations
{
    public partial class AddTripBudget : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "Trips",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Trips",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Budget",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Trips");
        }
    }
}
