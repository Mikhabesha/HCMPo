using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCMPo.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeAllowanceAndDeductionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeAllowances",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AllowanceTypeId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAllowances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeAllowances_AllowanceTypes_AllowanceTypeId",
                        column: x => x.AllowanceTypeId,
                        principalTable: "AllowanceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeAllowances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeDeductions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DeductionTypeId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeDeductions_DeductionTypes_DeductionTypeId",
                        column: x => x.DeductionTypeId,
                        principalTable: "DeductionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeDeductions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4172));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "10",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4268));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "11",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4273));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "12",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4279));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "13",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4285));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "14",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4290));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4180));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4186));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "4",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4192));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "5",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4199));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "6",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4204));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "7",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4210));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "8",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4218));

            migrationBuilder.UpdateData(
                table: "Directorates",
                keyColumn: "Id",
                keyValue: "9",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4262));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(443));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(454));

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(465));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(586));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(595));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(616));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4326));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4333));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4339));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "4",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4345));

            migrationBuilder.UpdateData(
                table: "RoleHierarchies",
                keyColumn: "Id",
                keyValue: "5",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 20, 21, 18, 42, 874, DateTimeKind.Utc).AddTicks(4351));

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAllowances_AllowanceTypeId",
                table: "EmployeeAllowances",
                column: "AllowanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAllowances_EmployeeId",
                table: "EmployeeAllowances",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDeductions_DeductionTypeId",
                table: "EmployeeDeductions",
                column: "DeductionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDeductions_EmployeeId",
                table: "EmployeeDeductions",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeAllowances");

            migrationBuilder.DropTable(
                name: "EmployeeDeductions");

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
    }
}
