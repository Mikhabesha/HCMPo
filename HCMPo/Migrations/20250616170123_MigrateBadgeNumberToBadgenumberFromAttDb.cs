using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCMPo.Migrations
{
    /// <inheritdoc />
    public partial class MigrateBadgeNumberToBadgenumberFromAttDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 17, 1, 22, 137, DateTimeKind.Utc).AddTicks(6670));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 17, 1, 22, 137, DateTimeKind.Utc).AddTicks(6679));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 17, 1, 22, 137, DateTimeKind.Utc).AddTicks(6684));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 17, 1, 22, 137, DateTimeKind.Utc).AddTicks(6932));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 17, 1, 22, 137, DateTimeKind.Utc).AddTicks(6950));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 17, 1, 22, 137, DateTimeKind.Utc).AddTicks(6956));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 17, 1, 22, 137, DateTimeKind.Utc).AddTicks(6979));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 17, 1, 22, 137, DateTimeKind.Utc).AddTicks(6989));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 17, 1, 22, 137, DateTimeKind.Utc).AddTicks(6996));

            // In the Up() method, add logic to merge duplicate employees:
            // 1. For each employee with the same email but different BadgeNumber, migrate payroll and attendance records to the employee with the correct BADGENUMBER, then delete the duplicate.
            // (Pseudocode/SQL for EF migration)
            // Example:
            // foreach (var group in Employees.GroupBy(e => e.Email).Where(g => g.Count() > 1))
            // {
            //     var correct = group.FirstOrDefault(e => e.BadgeNumber == /* BADGENUMBER logic */);
            //     foreach (var dup in group.Where(e => e != correct))
            //     {
            //         // Update Payrolls
            //         Payrolls.Where(p => p.EmployeeId == dup.Id).ToList().ForEach(p => p.EmployeeId = correct.Id);
            //         // Update Attendances
            //         Attendances.Where(a => a.EmployeeId == dup.Id).ToList().ForEach(a => a.EmployeeId = correct.Id);
            //         // Remove duplicate employee
            //         Employees.Remove(dup);
            //     }
            // }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 12, 58, 29, 619, DateTimeKind.Utc).AddTicks(8431));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 12, 58, 29, 619, DateTimeKind.Utc).AddTicks(8439));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 16, 12, 58, 29, 619, DateTimeKind.Utc).AddTicks(8450));

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
    }
}
