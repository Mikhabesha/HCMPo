using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCMPo.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "JobTitles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "JobTitles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "JobTitles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "JobTitles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "JobTitles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "JobTitles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "JobTitles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "Employees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Departments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Departments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Departments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "Departments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "Code", "CreatedAt", "CreatedBy", "IsActive", "ModifiedAt", "ModifiedBy" },
                values: new object[] { null, new DateTime(2025, 6, 12, 12, 20, 5, 654, DateTimeKind.Utc).AddTicks(2359), null, true, null, null });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "Code", "CreatedAt", "CreatedBy", "IsActive", "ModifiedAt", "ModifiedBy" },
                values: new object[] { null, new DateTime(2025, 6, 12, 12, 20, 5, 654, DateTimeKind.Utc).AddTicks(2371), null, true, null, null });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "Code", "CreatedAt", "CreatedBy", "IsActive", "ModifiedAt", "ModifiedBy" },
                values: new object[] { null, new DateTime(2025, 6, 12, 12, 20, 5, 654, DateTimeKind.Utc).AddTicks(2377), null, true, null, null });

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "CreatedAt", "CreatedBy", "Grade", "IsActive", "ModifiedAt", "ModifiedBy" },
                values: new object[] { new DateTime(2025, 6, 12, 12, 20, 5, 654, DateTimeKind.Utc).AddTicks(2523), null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "CreatedAt", "CreatedBy", "Grade", "IsActive", "ModifiedAt", "ModifiedBy" },
                values: new object[] { new DateTime(2025, 6, 12, 12, 20, 5, 654, DateTimeKind.Utc).AddTicks(2530), null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "JobTitles",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "CreatedAt", "CreatedBy", "Grade", "IsActive", "ModifiedAt", "ModifiedBy" },
                values: new object[] { new DateTime(2025, 6, 12, 12, 20, 5, 654, DateTimeKind.Utc).AddTicks(2542), null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "1",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 12, 12, 20, 5, 654, DateTimeKind.Utc).AddTicks(2560));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "2",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 12, 12, 20, 5, 654, DateTimeKind.Utc).AddTicks(2570));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: "3",
                column: "CreatedAt",
                value: new DateTime(2025, 6, 12, 12, 20, 5, 654, DateTimeKind.Utc).AddTicks(2577));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Departments");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "JobTitles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

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
    }
}
