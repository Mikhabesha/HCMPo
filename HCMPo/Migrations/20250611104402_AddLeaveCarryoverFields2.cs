using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCMPo.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveCarryoverFields2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 11, 10, 43, 58, 587, DateTimeKind.Utc).AddTicks(9168));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 11, 10, 43, 58, 587, DateTimeKind.Utc).AddTicks(9178));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 11, 10, 43, 58, 587, DateTimeKind.Utc).AddTicks(9185));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 11, 8, 9, 8, 317, DateTimeKind.Utc).AddTicks(6213));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 11, 8, 9, 8, 317, DateTimeKind.Utc).AddTicks(6223));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 11, 8, 9, 8, 317, DateTimeKind.Utc).AddTicks(6238));
        }
    }
}
