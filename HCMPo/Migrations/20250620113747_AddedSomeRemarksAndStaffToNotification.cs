using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCMPo.Migrations
{
    /// <inheritdoc />
    public partial class AddedSomeRemarksAndStaffToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "LeaveRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2433));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "10",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2551));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "11",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2557));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "12",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2564));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "13",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2571));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "14",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2579));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2443));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2450));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "4",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2457));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "5",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2463));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "6",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2473));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "7",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2479));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "8",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2486));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "9",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2546));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 162, DateTimeKind.Utc).AddTicks(5971));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 162, DateTimeKind.Utc).AddTicks(5982));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 162, DateTimeKind.Utc).AddTicks(5995));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 162, DateTimeKind.Utc).AddTicks(6221));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 162, DateTimeKind.Utc).AddTicks(6233));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 162, DateTimeKind.Utc).AddTicks(6242));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2633));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2641));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2648));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "4",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2655));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "5",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 11, 37, 43, 163, DateTimeKind.Utc).AddTicks(2661));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "LeaveRequests");

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(1884));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "10",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(2089));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "11",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(2099));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "12",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(2108));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "13",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(2117));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "14",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(2125));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(1897));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(1907));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "4",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(1916));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "5",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(1925));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "6",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(2048));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "7",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(2063));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "8",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(2072));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "9",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(2081));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 343, DateTimeKind.Utc).AddTicks(6413));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 343, DateTimeKind.Utc).AddTicks(6423));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 343, DateTimeKind.Utc).AddTicks(6429));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 343, DateTimeKind.Utc).AddTicks(6683));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 343, DateTimeKind.Utc).AddTicks(6707));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 343, DateTimeKind.Utc).AddTicks(6715));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(2229));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(2241));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(2299));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "4",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(2309));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "5",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 19, 8, 1, 57, 344, DateTimeKind.Utc).AddTicks(2317));
        }
    }
}
