using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HCMPo.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationStructureWithDataMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create new tables first (without foreign key constraints to Employees)
            migrationBuilder.CreateTable(
                name: "Directorates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    ParentDirectorateId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directorates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Directorates_Directorates_ParentDirectorateId",
                        column: x => x.ParentDirectorateId,
                        principalTable: "Directorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeMigrationLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourceSystemId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceSystemName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarningMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedDirectorate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedOrganizationUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedRole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeMigrationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUnits",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TeamLeaderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectorId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUnits_OrganizationUnits_ParentId",
                        column: x => x.ParentId,
                        principalTable: "OrganizationUnits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoleHierarchies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HierarchyLevel = table.Column<int>(type: "int", nullable: false),
                    MaxApprovalLevel = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CanApproveLeave = table.Column<bool>(type: "bit", nullable: false),
                    CanManageAttendance = table.Column<bool>(type: "bit", nullable: false),
                    CanAccessPayroll = table.Column<bool>(type: "bit", nullable: false),
                    CanManageEmployees = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleHierarchies", x => x.Id);
                });

            // Step 2: Migrate data from Departments to OrganizationUnits
            migrationBuilder.Sql(@"
                INSERT INTO OrganizationUnits (Id, Name, Type, IsActive, CreatedAt, Description)
                SELECT 
                    Id,
                    Name,
                    0, -- Type = Department
                    1, -- IsActive = true
                    GETDATE(),
                    Description
                FROM Departments
            ");

            // Step 3: Add new column to Employees table
            migrationBuilder.AddColumn<string>(
                name: "DirectorateId",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: true);

            // Step 4: Create EmployeePositions table
            migrationBuilder.CreateTable(
                name: "EmployeePositions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DirectorateId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OrganizationUnitId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RoleHierarchyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SupervisorEmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrentPosition = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimaryPosition = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeePositions_Directorates_DirectorateId",
                        column: x => x.DirectorateId,
                        principalTable: "Directorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeePositions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeePositions_Employees_SupervisorEmployeeId",
                        column: x => x.SupervisorEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeePositions_OrganizationUnits_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalTable: "OrganizationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeePositions_RoleHierarchies_RoleHierarchyId",
                        column: x => x.RoleHierarchyId,
                        principalTable: "RoleHierarchies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Step 5: Insert seed data
            migrationBuilder.InsertData(
                table: "Directorates",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Level", "ModifiedAt", "ModifiedBy", "Name", "ParentDirectorateId" },
                values: new object[,]
                {
                    { "1", "OCG", new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4760), null, "", true, 1, null, null, "Office of the Director General", null },
                    { "10", "ITD", new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4835), null, "", true, 1, null, null, "Information Technology Directorate", null },
                    { "11", "PPD", new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4843), null, "", true, 1, null, null, "Planning and Performance Directorate", null },
                    { "12", "CCD", new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4849), null, "", true, 1, null, null, "Corporate Communication Directorate", null },
                    { "13", "RID", new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4855), null, "", true, 1, null, null, "Policy and Research Directorate", null },
                    { "14", "ROD", new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4861), null, "", true, 1, null, null, "Regional Operations Directorate", null },
                    { "2", "RMD", new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4770), null, "", true, 1, null, null, "Operations Management Directorate", null },
                    { "3", "COD", new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4782), null, "", true, 1, null, null, "Member Services Directorate", null },
                    { "4", "TAD", new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4789), null, "", true, 1, null, null, "Benefits Administration Directorate", null },
                    { "5", "IID", new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4795), null, "", true, 1, null, null, "Investigation and Intelligence Directorate", null },
                    { "6", "LAD", new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4802), null, "", true, 1, null, null, "Legal Affairs Directorate", null },
                    { "7", "IAD", new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4808), null, "", true, 1, null, null, "Internal Audit Directorate", null },
                    { "8", "HRD", new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4814), null, "", true, 1, null, null, "Human Resources Directorate", null },
                    { "9", "FPD", new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4828), null, "", true, 1, null, null, "Finance and Procurement Directorate", null }
                });

            migrationBuilder.InsertData(
                table: "RoleHierarchies",
                columns: new[] { "Id", "RoleName", "DisplayName", "Description", "HierarchyLevel", "MaxApprovalLevel", "IsActive", "CanApproveLeave", "CanManageAttendance", "CanAccessPayroll", "CanManageEmployees", "CreatedAt" },
                values: new object[,]
                {
                    { "1", "Employee", "Employee", "Regular employee with basic access", 1, 0, true, false, false, false, false, new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4867) },
                    { "2", "TeamLeader", "Team Leader", "Team leader with approval capabilities", 2, 1, true, true, true, false, false, new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4873) },
                    { "3", "Manager", "Manager", "Manager with extended permissions", 3, 2, true, true, true, true, false, new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4879) },
                    { "4", "Director", "Director", "Director with full access", 4, 3, true, true, true, true, true, new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4885) },
                    { "5", "Admin", "Administrator", "System administrator with all permissions", 5, 5, true, true, true, true, true, new DateTime(2025, 6, 18, 19, 19, 4, 618, DateTimeKind.Utc).AddTicks(4891) }
                });

            // Step 6: Now safely drop the foreign key constraint and rename the column
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "Employees",
                newName: "OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                newName: "IX_Employees_OrganizationUnitId");

            // Step 7: Add the new foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Employees_OrganizationUnits_OrganizationUnitId",
                table: "Employees",
                column: "OrganizationUnitId",
                principalTable: "OrganizationUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Step 8: Finally drop the old Departments table
            migrationBuilder.DropTable(
                name: "Departments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_OrganizationUnits_OrganizationUnitId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "EmployeeMigrationLogs");

            migrationBuilder.DropTable(
                name: "EmployeePositions");

            migrationBuilder.DropTable(
                name: "Directorates");

            migrationBuilder.DropTable(
                name: "OrganizationUnits");

            migrationBuilder.DropTable(
                name: "RoleHierarchies");

            migrationBuilder.RenameColumn(
                name: "OrganizationUnitId",
                table: "Employees",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_OrganizationUnitId",
                table: "Employees",
                newName: "IX_Employees_DepartmentId");

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DirectorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TeamLeaderId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "DirectorId", "IsActive", "ModifiedAt", "ModifiedBy", "Name", "TeamLeaderId" },
                values: new object[,]
                {
                    { "1", null, new DateTime(2025, 6, 16, 17, 1, 22, 137, DateTimeKind.Utc).AddTicks(6670), null, "HR Department", null, true, null, null, "Human Resources", null },
                    { "2", null, new DateTime(2025, 6, 16, 17, 1, 22, 137, DateTimeKind.Utc).AddTicks(6679), null, "IT Department", null, true, null, null, "Information Technology", null },
                    { "3", null, new DateTime(2025, 6, 16, 17, 1, 22, 137, DateTimeKind.Utc).AddTicks(6684), null, "Finance Department", null, true, null, null, "Finance", null }
                });

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

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
