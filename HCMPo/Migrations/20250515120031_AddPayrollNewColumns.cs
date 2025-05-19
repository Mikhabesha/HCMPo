using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCMPo.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollNewColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Allowance",
                table: "Payrolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AttendanceDeductedSalary",
                table: "Payrolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetPaySalary",
                table: "Payrolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PensionEmployee",
                table: "Payrolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PensionEmployer",
                table: "Payrolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeduction",
                table: "Payrolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSalary",
                table: "Payrolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "PayrollConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeePensionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmployerPensionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LateDeductionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AbsentDeductionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollDeductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayrollId1 = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollDeductions_Payrolls_PayrollId1",
                        column: x => x.PayrollId1,
                        principalTable: "Payrolls",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 15, 12, 0, 27, 874, DateTimeKind.Utc).AddTicks(6920));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 15, 12, 0, 27, 874, DateTimeKind.Utc).AddTicks(6932));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 15, 12, 0, 27, 874, DateTimeKind.Utc).AddTicks(6945));

            migrationBuilder.CreateIndex(
                name: "IX_PayrollDeductions_PayrollId1",
                table: "PayrollDeductions",
                column: "PayrollId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayrollConfigurations");

            migrationBuilder.DropTable(
                name: "PayrollDeductions");

            migrationBuilder.DropColumn(
                name: "Allowance",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "AttendanceDeductedSalary",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "NetPaySalary",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "PensionEmployee",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "PensionEmployer",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "TotalDeduction",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "TotalSalary",
                table: "Payrolls");

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 15, 8, 28, 6, 297, DateTimeKind.Utc).AddTicks(6003));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 15, 8, 28, 6, 297, DateTimeKind.Utc).AddTicks(6014));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 15, 8, 28, 6, 297, DateTimeKind.Utc).AddTicks(6021));
        }
    }
}
