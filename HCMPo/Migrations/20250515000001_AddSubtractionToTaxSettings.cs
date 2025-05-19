using Microsoft.EntityFrameworkCore.Migrations;

namespace HCMPo.Migrations
{
    public partial class AddSubtractionToTaxSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Subtraction",
                table: "TaxSettings",
                type: "decimal(18,2)",
                nullable: true
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subtraction",
                table: "TaxSettings"
            );
        }
    }
} 