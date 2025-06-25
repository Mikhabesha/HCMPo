using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCMPo.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationUnitIdToJobTitleNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrganizationUnitId",
                table: "JobTitles",
                type: "nvarchar(450)",
                nullable: true);

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
                columns: new[] { "CreatedAt", "OrganizationUnitId" },
                values: new object[] { new DateTime(2025, 6, 19, 8, 1, 57, 343, DateTimeKind.Utc).AddTicks(6413), null });

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "CreatedAt", "OrganizationUnitId" },
                values: new object[] { new DateTime(2025, 6, 19, 8, 1, 57, 343, DateTimeKind.Utc).AddTicks(6423), null });

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "CreatedAt", "OrganizationUnitId" },
                values: new object[] { new DateTime(2025, 6, 19, 8, 1, 57, 343, DateTimeKind.Utc).AddTicks(6429), null });

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

            migrationBuilder.CreateIndex(
                name: "IX_JobTitles_OrganizationUnitId",
                table: "JobTitles",
                column: "OrganizationUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobTitles_OrganizationUnits_OrganizationUnitId",
                table: "JobTitles",
                column: "OrganizationUnitId",
                principalTable: "OrganizationUnits",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobTitles_OrganizationUnits_OrganizationUnitId",
                table: "JobTitles");

            migrationBuilder.DropIndex(
                name: "IX_JobTitles_OrganizationUnitId",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "OrganizationUnitId",
                table: "JobTitles");

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4760));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "10",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4835));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "11",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4843));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "12",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4849));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "13",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4855));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "14",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4861));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4770));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4782));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "4",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4789));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "5",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4795));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "6",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4802));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "7",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4808));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "8",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4814));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "9",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4828));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 617, DateTimeKind.Utc).AddTicks(8908));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 617, DateTimeKind.Utc).AddTicks(8936));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 617, DateTimeKind.Utc).AddTicks(8943));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 617, DateTimeKind.Utc).AddTicks(9174));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 617, DateTimeKind.Utc).AddTicks(9196));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 617, DateTimeKind.Utc).AddTicks(9204));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4959));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4967));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4974));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "4",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4981));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "5",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4988));
        }
    }
}
