using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCMPo.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxSettings2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "TaxSettings");

            migrationBuilder.RenameColumn(
                name: "DefaultPercentage",
                table: "TaxSettings",
                newName: "Percentage");

            migrationBuilder.AddColumn<decimal>(
                name: "MaxSalary",
                table: "TaxSettings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinSalary",
                table: "TaxSettings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "TaxSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 12, 8, 54, 6, 238, DateTimeKind.Utc).AddTicks(8085));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 12, 8, 54, 6, 238, DateTimeKind.Utc).AddTicks(8097));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 12, 8, 54, 6, 238, DateTimeKind.Utc).AddTicks(8104));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxSalary",
                table: "TaxSettings");

            migrationBuilder.DropColumn(
                name: "MinSalary",
                table: "TaxSettings");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TaxSettings");

            migrationBuilder.RenameColumn(
                name: "Percentage",
                table: "TaxSettings",
                newName: "DefaultPercentage");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TaxSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 12, 8, 38, 48, 258, DateTimeKind.Utc).AddTicks(2827));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 12, 8, 38, 48, 258, DateTimeKind.Utc).AddTicks(2836));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 12, 8, 38, 48, 258, DateTimeKind.Utc).AddTicks(2843));
        }
    }
}
