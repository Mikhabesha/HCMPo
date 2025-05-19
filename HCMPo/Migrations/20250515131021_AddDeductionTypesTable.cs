using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HCMPo.Migrations
{
    /// <inheritdoc />
    public partial class AddDeductionTypesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "PayrollDeductions",
                newName: "DeductionTypeId");

            migrationBuilder.CreateTable(
                name: "DeductionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeductionTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DeductionTypes",
                columns: new[] { "Id", "DisplayName", "IsActive", "Name", "Order" },
                values: new object[,]
                {
                    { 1, "Cost Sharing", true, "CostSharing", 1 },
                    { 2, "Some Contribution", true, "SomeContribution", 2 },
                    { 3, "Saving", true, "Saving", 3 },
                    { 4, "HIV", true, "HIV", 4 },
                    { 5, "Defence Force", true, "DefenceForce", 5 },
                    { 6, "Health", true, "Health", 6 },
                    { 7, "Prosperity Party", true, "ProsperityParty", 7 },
                    { 8, "Return from Salary", true, "ReturnFromSalary", 8 },
                    { 9, "Red Cross", true, "RedCross", 9 }
                });

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 15, 13, 10, 17, 681, DateTimeKind.Utc).AddTicks(8109));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 15, 13, 10, 17, 681, DateTimeKind.Utc).AddTicks(8129));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 15, 13, 10, 17, 681, DateTimeKind.Utc).AddTicks(8136));

            migrationBuilder.CreateIndex(
                name: "IX_PayrollDeductions_DeductionTypeId",
                table: "PayrollDeductions",
                column: "DeductionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollDeductions_DeductionTypes_DeductionTypeId",
                table: "PayrollDeductions",
                column: "DeductionTypeId",
                principalTable: "DeductionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayrollDeductions_DeductionTypes_DeductionTypeId",
                table: "PayrollDeductions");

            migrationBuilder.DropTable(
                name: "DeductionTypes");

            migrationBuilder.DropIndex(
                name: "IX_PayrollDeductions_DeductionTypeId",
                table: "PayrollDeductions");

            migrationBuilder.RenameColumn(
                name: "DeductionTypeId",
                table: "PayrollDeductions",
                newName: "Type");

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 15, 12, 20, 47, 921, DateTimeKind.Utc).AddTicks(3237));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 15, 12, 20, 47, 921, DateTimeKind.Utc).AddTicks(3252));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 5, 15, 12, 20, 47, 921, DateTimeKind.Utc).AddTicks(3268));
        }
    }
}
