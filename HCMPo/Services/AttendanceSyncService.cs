using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HCMPo.Models;
using System.Collections.Concurrent;
using HCMPo.Data;

namespace HCMPo.Services
{
    public class AttendanceSyncService : IAttendanceSyncService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttendanceSyncService> _logger;
        private static ConcurrentDictionary<string, int> SyncProgress = new ConcurrentDictionary<string, int>();
        private static ConcurrentDictionary<string, int> SyncTotal = new ConcurrentDictionary<string, int>();

        public AttendanceSyncService(
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<AttendanceSyncService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        public int GetSyncProgress(string key)
        {
            SyncProgress.TryGetValue(key, out int value);
            return value;
        }
        public int GetSyncTotal(string key)
        {
            SyncTotal.TryGetValue(key, out int value);
            return value;
        }

        public async Task<List<ZKTimeAttendance>> GetRawAttendanceDataAsync(DateTime? fromDate = null)
        {
            var attendanceRecords = new List<ZKTimeAttendance>();
            
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("ZKTimeConnection")))
                {
                    await connection.OpenAsync();

                    var query = @"
                        SELECT 
                            cio.USERID,
                            u.NAME as UserName,
                            cio.CHECKTIME,
                            cio.CHECKTYPE,
                            cio.VERIFYCODE as VerifyMode,
                            cio.WorkCode,
                            cio.SENSORID as DeviceId,
                            d.DEPTNAME as DeviceName
                        FROM CHECKINOUT cio
                        LEFT JOIN USERINFO u ON cio.USERID = u.USERID
                        LEFT JOIN ORGANIZATIONUNITS d ON u.DEFAULTDEPTID = d.Id
                        WHERE (@FromDate IS NULL OR cio.CHECKTIME >= @FromDate)
                        ORDER BY cio.CHECKTIME";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FromDate", fromDate ?? (object)DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var record = new ZKTimeAttendance
                                {
                                    UserId = reader.GetInt32(reader.GetOrdinal("USERID")),
                                    UserName = reader.GetString(reader.GetOrdinal("UserName")),
                                    CheckTime = reader.GetDateTime(reader.GetOrdinal("CHECKTIME")),
                                    CheckType = reader.GetString(reader.GetOrdinal("CHECKTYPE")),
                                    VerifyMode = reader.GetInt32(reader.GetOrdinal("VerifyMode")).ToString(),
                                    WorkCode = reader.GetString(reader.GetOrdinal("WorkCode")),
                                    DeviceId = reader.GetString(reader.GetOrdinal("DeviceId")),
                                    DeviceName = reader.GetString(reader.GetOrdinal("DeviceName"))
                                };

                                attendanceRecords.Add(record);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving raw attendance data");
                throw;
            }

            return attendanceRecords;
        }

        public async Task SyncAttendanceDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting attendance data synchronization");

                // Get the last sync time from our database
                var lastSyncTime = await _context.SyncLogs
                    .OrderByDescending(s => s.SyncTime)
                    .Select(s => s.SyncTime)
                    .FirstOrDefaultAsync();

                if (lastSyncTime == default)
                {
                    lastSyncTime = DateTime.Now.AddDays(-1); // Default to 1 day ago if no previous sync
                }

                var currentTime = DateTime.Now;

                // Get raw attendance data from ZKTeco database
                var rawAttendanceData = await GetRawAttendanceDataAsync(lastSyncTime);

                if (!rawAttendanceData.Any())
                {
                    _logger.LogInformation("No new attendance records to sync");
                    return;
                }

                var newRecords = new List<Attendance>();
                var employeeMap = new Dictionary<int, string>(); // ZKTime UserID to HCMDb EmployeeId

                foreach (var rawRecord in rawAttendanceData)
                {
                    // Get or create mapping between ZKTime BADGENUMBER and HCMDb EmployeeId
                    if (!employeeMap.TryGetValue(rawRecord.UserId, out var employeeId))
                    {
                        // Fetch BADGENUMBER for this USERID from USERINFO
                        string badgeNumber = null;
                        using (var connection = new SqlConnection(_configuration.GetConnectionString("ZKTimeConnection")))
                        {
                            await connection.OpenAsync();
                            var badgeQuery = "SELECT u.BADGENUMBER FROM USERINFO u WHERE u.USERID = @UserId";
                            using (var badgeCmd = new SqlCommand(badgeQuery, connection))
                            {
                                badgeCmd.Parameters.AddWithValue("@UserId", rawRecord.UserId);
                                var result = await badgeCmd.ExecuteScalarAsync();
                                badgeNumber = result?.ToString();
                            }
                        }
                        if (string.IsNullOrWhiteSpace(badgeNumber))
                        {
                            badgeNumber = rawRecord.UserId.ToString();
                        }
                        var employee = await _context.Employees
                            .FirstOrDefaultAsync(e => e.BadgeNumber == badgeNumber);
                        if (employee == null)
                        {
                            _logger.LogWarning($"Employee with BADGENUMBER {badgeNumber} not found in HCMDb");
                            continue;
                        }
                        employeeId = employee.Id;
                        employeeMap[rawRecord.UserId] = employeeId;
                    }

                    var attendance = new Attendance
                    {
                        EmployeeId = employeeId,
                        CheckInTime = rawRecord.CheckTime,
                        CheckInTimeEt = new EthiopianCalendar(rawRecord.CheckTime).ToString(),
                        Status = DetermineAttendanceStatus(rawRecord),
                        DeviceId = rawRecord.DeviceId,
                        MemoInfo = rawRecord.WorkCode,
                        PunchType = rawRecord.CheckType,
                        VerificationMode = rawRecord.VerifyMode,
                        WorkCode = rawRecord.WorkCode,
                        CreatedAt = DateTime.UtcNow
                    };

                    newRecords.Add(attendance);
                }

                if (newRecords.Any())
                {
                    await _context.Attendances.AddRangeAsync(newRecords);
                    await _context.SaveChangesAsync();

                    // Log successful sync
                    await _context.SyncLogs.AddAsync(new SyncLog
                    {
                        SyncTime = currentTime,
                        RecordsSynced = newRecords.Count,
                        Status = "Success"
                    });
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Successfully synced {newRecords.Count} attendance records");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during attendance data synchronization");

                // Log failed sync
                await _context.SyncLogs.AddAsync(new SyncLog
                {
                    SyncTime = DateTime.Now,
                    RecordsSynced = 0,
                    Status = "Failed",
                    ErrorMessage = ex.Message
                });
                await _context.SaveChangesAsync();
                throw;
            }
        }

        private AttendanceStatus DetermineAttendanceStatus(ZKTimeAttendance record)
        {
            
            return AttendanceStatus.Present;
        }

        public async Task<int> SyncEmployeesFromAttDbAsync(string progressKey = null)
        {
            int addedCount = 0;
            int batchSize = 500;
            var connectionString = _configuration.GetConnectionString("ZKTimeConnection") ?? throw new InvalidOperationException("ZKTimeConnection connection string is not configured");
            var existingOrganizationUnits = await _context.OrganizationUnits.AsNoTracking()
                .GroupBy(ou => ou.Id).Select(g => g.First()).ToDictionaryAsync(ou => ou.Id);
            var existingEmployees = await _context.Employees.AsNoTracking()
                .GroupBy(e => e.BadgeNumber).Select(g => g.First()).ToDictionaryAsync(e => e.BadgeNumber);
            var newOrganizationUnits = new List<OrganizationUnit>();
            var newEmployees = new List<Employee>();
            int total = 0;
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT u.USERID, u.BADGENUMBER, u.NAME, u.DEFAULTDEPTID, u.GENDER, u.BIRTHDAY FROM USERINFO u";
                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var allRows = new List<(string userId, string badgeNumber, string name, string deptId, string gender, DateTime birthday, string badgeForEmail)>();
                    while (await reader.ReadAsync())
                    {
                        var userId = reader["USERID"].ToString() ?? "";
                        var badgeNumber = reader["BADGENUMBER"].ToString() ?? "";
                        if (string.IsNullOrWhiteSpace(badgeNumber))
                        {
                            badgeNumber = userId;
                        }
                        var name = reader["NAME"].ToString() ?? "Unknown";
                        var deptId = reader["DEFAULTDEPTID"].ToString() ?? "";
                        var gender = reader["GENDER"].ToString() ?? "";
                        var birthday = reader["BIRTHDAY"] != DBNull.Value ? Convert.ToDateTime(reader["BIRTHDAY"]) : DateTime.Now;
                        var badgeForEmail = badgeNumber;
                        allRows.Add((userId, badgeNumber, name, deptId, gender, birthday, badgeForEmail));
                    }
                    total = allRows.Count;
                    if (progressKey != null) SyncTotal[progressKey] = total;
                    int processed = 0;
                    foreach (var row in allRows)
                    {
                        var existingEmployee = existingEmployees.ContainsKey(row.badgeNumber) ? existingEmployees[row.badgeNumber] : null;
                        var nameParts = row.name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                        var firstName = nameParts.Length > 0 ? nameParts[0] : "Unknown";
                        var lastName = nameParts.Length > 1 ? nameParts[1] : "";
                        if (int.TryParse(firstName, out _) && string.IsNullOrEmpty(lastName))
                        {
                            firstName = "Unknown";
                            lastName = "";
                        }
                        if (existingEmployee != null)
                        {
                            // Update existing employee's info
                            existingEmployee.FirstName = firstName;
                            existingEmployee.LastName = lastName;
                            existingEmployee.OrganizationUnitId = row.deptId;
                            existingEmployee.Gender = row.gender;
                            existingEmployee.DateOfBirth = row.birthday;
                            existingEmployee.Email = $"{row.badgeForEmail}@hcm.com";
                            // Optionally update other fields as needed
                            _context.Employees.Update(existingEmployee);
                            continue;
                        }
                        if (!existingOrganizationUnits.ContainsKey(row.deptId) && !newOrganizationUnits.Any(ou => ou.Id == row.deptId))
                        {
                            newOrganizationUnits.Add(new OrganizationUnit { Id = row.deptId, Name = $"Dept {row.deptId}", Description = "Imported from Att_db" });
                        }
                        newEmployees.Add(new Employee
                        {
                            BadgeNumber = row.badgeNumber,
                            FirstName = firstName,
                            LastName = lastName,
                            OrganizationUnitId = row.deptId,
                            Gender = row.gender,
                            DateOfBirth = row.birthday,
                            Email = $"{row.badgeForEmail}@hcm.com",
                            PhoneNumber = "N/A",
                            HireDate = DateTime.Now,
                            Salary = 0,
                            JobTitleId = _context.JobTitles.FirstOrDefault()?.Id ?? "1",
                            BasicSalary = 0,
                            EmploymentDate = DateTime.Now,
                            Status = EmploymentStatus.Active
                        });
                        addedCount++;
                        processed++;
                        if (progressKey != null) SyncProgress[progressKey] = processed;
                        if (newEmployees.Count >= batchSize)
                        {
                            if (newOrganizationUnits.Any())
                            {
                                await _context.OrganizationUnits.AddRangeAsync(newOrganizationUnits);
                                newOrganizationUnits.Clear();
                            }
                            await _context.Employees.AddRangeAsync(newEmployees);
                            await _context.SaveChangesAsync();
                            newEmployees.Clear();
                        }
                    }
                    if (newOrganizationUnits.Any())
                        await _context.OrganizationUnits.AddRangeAsync(newOrganizationUnits);
                    if (newEmployees.Any())
                        await _context.Employees.AddRangeAsync(newEmployees);
                    await _context.SaveChangesAsync();
                    if (progressKey != null) SyncProgress[progressKey] = processed;
                }
            }
            return addedCount;
        }

        public async Task<int> SyncAttendanceFromAttDbAsync(string progressKey = null)
        {
            int addedCount = 0;
            int batchSize = 500;
            var connectionString = _configuration.GetConnectionString("ZKTimeConnection") ?? throw new InvalidOperationException("ZKTimeConnection connection string is not configured");
            var employees = await _context.Employees.AsNoTracking()
                .GroupBy(e => e.BadgeNumber).Select(g => g.First()).ToDictionaryAsync(e => e.BadgeNumber);
            var newAttendances = new List<Attendance>();
            int total = 0;

            // Bulk load all existing attendance keys for fast duplicate checking
            var existingAttendanceKeys = new HashSet<string>(
                await _context.Attendances.AsNoTracking()
                    .Select(a => a.EmployeeId + "|" + a.CheckInTime.ToString("o") + "|" + a.PunchType)
                    .ToListAsync()
            );

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT c.USERID, u.BADGENUMBER, c.CHECKTIME, c.CHECKTYPE, c.VERIFYCODE, c.SENSORID, c.WorkCode FROM CHECKINOUT c LEFT JOIN USERINFO u ON c.USERID = u.USERID";
                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var allRows = new List<(string badgeNumber, DateTime checkTime, string checkType, string verifyCode, string sensorId, string workCode)>();
                    while (await reader.ReadAsync())
                    {
                        var badgeNumber = reader["BADGENUMBER"].ToString() ?? "";
                        if (string.IsNullOrWhiteSpace(badgeNumber))
                        {
                            badgeNumber = reader["USERID"].ToString() ?? "";
                        }
                        var checkTime = reader["CHECKTIME"] != DBNull.Value ? Convert.ToDateTime(reader["CHECKTIME"]) : DateTime.Now;
                        var checkType = reader["CHECKTYPE"].ToString() ?? "";
                        var verifyCode = reader["VERIFYCODE"].ToString() ?? "";
                        var sensorId = reader["SENSORID"].ToString() ?? "";
                        var workCode = reader["WorkCode"].ToString() ?? "";
                        allRows.Add((badgeNumber, checkTime, checkType, verifyCode, sensorId, workCode));
                    }
                    total = allRows.Count;
                    if (progressKey != null) SyncTotal[progressKey] = total;
                    int processed = 0;
                    foreach (var row in allRows)
                    {
                        if (!employees.TryGetValue(row.badgeNumber, out var employee)) continue;
                        string attKey = employee.Id + "|" + row.checkTime.ToString("o") + "|" + row.checkType;
                        if (existingAttendanceKeys.Contains(attKey) || newAttendances.Any(a => a.EmployeeId == employee.Id && a.CheckInTime == row.checkTime && a.PunchType == row.checkType)) continue;
                        newAttendances.Add(new Attendance
                        {
                            Id = Guid.NewGuid().ToString(),
                            EmployeeId = employee.Id,
                            CheckInTime = row.checkTime,
                            PunchType = row.checkType,
                            VerificationMode = row.verifyCode,
                            DeviceId = row.sensorId,
                            WorkCode = row.workCode,
                            Status = AttendanceStatus.Present,
                            CreatedAt = DateTime.UtcNow
                        });
                        addedCount++;
                        processed++;
                        if (progressKey != null) SyncProgress[progressKey] = processed;
                        if (newAttendances.Count >= batchSize)
                        {
                            await _context.Attendances.AddRangeAsync(newAttendances);
                            await _context.SaveChangesAsync();
                            // Add new keys to the set
                            foreach (var a in newAttendances)
                                existingAttendanceKeys.Add(a.EmployeeId + "|" + a.CheckInTime.ToString("o") + "|" + a.PunchType);
                            newAttendances.Clear();
                        }
                    }
                    if (newAttendances.Any())
                    {
                        await _context.Attendances.AddRangeAsync(newAttendances);
                        await _context.SaveChangesAsync();
                        foreach (var a in newAttendances)
                            existingAttendanceKeys.Add(a.EmployeeId + "|" + a.CheckInTime.ToString("o") + "|" + a.PunchType);
                    }
                    if (progressKey != null) SyncProgress[progressKey] = processed;
                }
            }
            return addedCount;
        }
    }
} 