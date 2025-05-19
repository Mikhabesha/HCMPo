using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HCMPo.Migrations
{
    public partial class AddProgressiveTaxBrackets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear existing income tax settings
            migrationBuilder.DeleteData(
                table: "TaxSettings",
                keyColumn: "Id",
                keyValue: 1);

            // Add new progressive tax brackets
            migrationBuilder.InsertData(
                table: "TaxSettings",
                columns: new[] { "Type", "MinSalary", "MaxSalary", "Percentage", "Subtraction", "IsActive" },
                values: new object[,]
                {
                    { 0, 0m, 600m, 0m, 0m, true }, // 0-600: Tax-Free
                    { 0, 601m, 3200m, 15m, 142.5m, true }, // 601-3200: 15%, subtract 142.5
                    { 0, 3201m, 5250m, 20m, 302.5m, true }, // 3201-5250: 20%, subtract 302.5
                    { 0, 5251m, 7800m, 25m, 565m, true }, // 5251-7800: 25%, subtract 565
                    { 0, 7801m, 10900m, 30m, 955m, true }, // 7801-10900: 30%, subtract 955
                    { 0, 10901m, null, 35m, 1500m, true }, // 10901+: 35%, subtract 1500
                    { 1, null, null, 7m, 0m, true } // Pension: 7%
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove all tax brackets
            migrationBuilder.DeleteData(
                table: "TaxSettings",
                keyColumn: "Id",
                keyValue: 1);

            // Restore original flat tax setting
            migrationBuilder.InsertData(
                table: "TaxSettings",
                columns: new[] { "Type", "MinSalary", "MaxSalary", "Percentage", "IsActive" },
                values: new object[] { 0, 0m, null, 15m, true });
        }
    }
} 