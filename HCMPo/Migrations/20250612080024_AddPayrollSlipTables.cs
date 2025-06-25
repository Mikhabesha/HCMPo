using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCMPo.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollSlipTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PayrollSlipConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CompanyAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CompanyPhone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanyEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanyLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankSlipTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ReportSlipTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ShowEmployeePhoto = table.Column<bool>(type: "bit", nullable: false),
                    ShowQRCode = table.Column<bool>(type: "bit", nullable: false),
                    ShowBarcode = table.Column<bool>(type: "bit", nullable: false),
                    FooterText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollSlipConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollStamps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ApplyOnBankSlip = table.Column<bool>(type: "bit", nullable: false),
                    ApplyOnReportSlip = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollStamps", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayrollSlipConfigurations");

            migrationBuilder.DropTable(
                name: "PayrollStamps");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Employees");

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
    }
}
