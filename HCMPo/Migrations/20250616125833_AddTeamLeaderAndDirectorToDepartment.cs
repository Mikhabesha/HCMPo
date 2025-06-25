using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCMPo.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamLeaderAndDirectorToDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DirectorId",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamLeaderId",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DirectorId",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamLeaderId",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "CreatedAt", "DirectorId", "TeamLeaderId" },
                values: new object[] { new DateTime(2025, 6, 16, 12, 58, 29, 619, DateTimeKind.Utc).AddTicks(8431), null, null });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "CreatedAt", "DirectorId", "TeamLeaderId" },
                values: new object[] { new DateTime(2025, 6, 16, 12, 58, 29, 619, DateTimeKind.Utc).AddTicks(8439), null, null });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "CreatedAt", "DirectorId", "TeamLeaderId" },
                values: new object[] { new DateTime(2025, 6, 16, 12, 58, 29, 619, DateTimeKind.Utc).AddTicks(8450), null, null });

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 12, 58, 29, 619, DateTimeKind.Utc).AddTicks(8602));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 12, 58, 29, 619, DateTimeKind.Utc).AddTicks(8609));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 12, 58, 29, 619, DateTimeKind.Utc).AddTicks(8615));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 12, 58, 29, 619, DateTimeKind.Utc).AddTicks(8639));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 12, 58, 29, 619, DateTimeKind.Utc).AddTicks(8649));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 12, 58, 29, 619, DateTimeKind.Utc).AddTicks(8657));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DirectorId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TeamLeaderId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DirectorId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "TeamLeaderId",
                table: "Departments");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 8, 42, 2, 835, DateTimeKind.Utc).AddTicks(6190));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 8, 42, 2, 835, DateTimeKind.Utc).AddTicks(6198));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 8, 42, 2, 835, DateTimeKind.Utc).AddTicks(6209));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 8, 42, 2, 835, DateTimeKind.Utc).AddTicks(6356));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 8, 42, 2, 835, DateTimeKind.Utc).AddTicks(6364));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 8, 42, 2, 835, DateTimeKind.Utc).AddTicks(6369));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 8, 42, 2, 835, DateTimeKind.Utc).AddTicks(6397));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 8, 42, 2, 835, DateTimeKind.Utc).AddTicks(6406));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 8, 42, 2, 835, DateTimeKind.Utc).AddTicks(6413));
        }
    }
}
