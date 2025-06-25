using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCMPo.Migrations
{
    /// <inheritdoc />
    public partial class AddFileSizeToPayrollStamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "PayrollStamps",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 12, 10, 43, 35, 185, DateTimeKind.Utc).AddTicks(8386));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 12, 10, 43, 35, 185, DateTimeKind.Utc).AddTicks(8396));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 12, 10, 43, 35, 185, DateTimeKind.Utc).AddTicks(8410));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "PayrollStamps");

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 12, 8, 0, 20, 168, DateTimeKind.Utc).AddTicks(4368));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 12, 8, 0, 20, 168, DateTimeKind.Utc).AddTicks(4380));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 12, 8, 0, 20, 168, DateTimeKind.Utc).AddTicks(4397));
        }
    }
}
