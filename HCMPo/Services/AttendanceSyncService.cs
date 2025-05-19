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
                        LEFT JOIN DEPARTMENTS d ON u.DEFAULTDEPTID = d.DEPTID
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
                    // Get or create mapping between ZKTime UserID and HCMDb EmployeeId
                    if (!employeeMap.TryGetValue(rawRecord.UserId, out var employeeId))
                    {
                        var employee = await _context.Employees
                            .FirstOrDefaultAsync(e => e.BadgeNumber == rawRecord.UserId.ToString());

                        if (employee == null)
                        {
                            _logger.LogWarning($"Employee with ZKTime UserID {rawRecord.UserId} not found in HCMDb");
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
            var connectionString = _configuration.GetConnectionString("ZKTimeConnection");
            var existingDepartments = await _context.Departments.AsNoTracking()
                .GroupBy(d => d.Id).Select(g => g.First()).ToDictionaryAsync(d => d.Id);
            var existingEmployees = await _context.Employees.AsNoTracking()
                .GroupBy(e => e.BadgeNumber).Select(g => g.First()).ToDictionaryAsync(e => e.BadgeNumber);
            var newDepartments = new List<Department>();
            var newEmployees = new List<Employee>();
            int total = 0;
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT USERID, BADGENUMBER, NAME, DEFAULTDEPTID, GENDER, BIRTHDAY FROM USERINFO";
                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var allRows = new List<(string userId, string badgeNumber, string name, string deptId, string gender, DateTime birthday, string badgeForEmail)>();
                    while (await reader.ReadAsync())
                    {
                        var userId = reader["USERID"].ToString();
                        var badgeNumber = userId;
                        var name = reader["NAME"].ToString() ?? "Unknown";
                        var deptId = reader["DEFAULTDEPTID"].ToString();
                        var gender = reader["GENDER"].ToString();
                        var birthday = reader["BIRTHDAY"] != DBNull.Value ? Convert.ToDateTime(reader["BIRTHDAY"]) : DateTime.Now;
                        var badgeForEmail = reader["BADGENUMBER"].ToString() ?? badgeNumber;
                        allRows.Add((userId, badgeNumber, name, deptId, gender, birthday, badgeForEmail));
                    }
                    total = allRows.Count;
                    if (progressKey != null) SyncTotal[progressKey] = total;
                    int processed = 0;
                    foreach (var row in allRows)
                    {
                        if (existingEmployees.ContainsKey(row.badgeNumber) || newEmployees.Any(e => e.BadgeNumber == row.badgeNumber)) continue;
                        if (!existingDepartments.ContainsKey(row.deptId) && !newDepartments.Any(d => d.Id == row.deptId))
                        {
                            newDepartments.Add(new Department { Id = row.deptId, Name = $"Dept {row.deptId}", Description = "Imported from Att_db" });
                        }
                        var nameParts = row.name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                        var firstName = nameParts.Length > 0 ? nameParts[0] : row.name;
                        var lastName = nameParts.Length > 1 ? nameParts[1] : nameParts[0];
                        newEmployees.Add(new Employee
                        {
                            BadgeNumber = row.badgeNumber,
                            FirstName = firstName,
                            LastName = lastName,
                            DepartmentId = row.deptId,
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
                            if (newDepartments.Any())
                            {
                                await _context.Departments.AddRangeAsync(newDepartments);
                                newDepartments.Clear();
                            }
                            await _context.Employees.AddRangeAsync(newEmployees);
                            await _context.SaveChangesAsync();
                            newEmployees.Clear();
                        }
                    }
                    if (newDepartments.Any())
                        await _context.Departments.AddRangeAsync(newDepartments);
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
            var connectionString = _configuration.GetConnectionString("ZKTimeConnection");
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
                var query = @"SELECT USERID, CHECKTIME, CHECKTYPE, VERIFYCODE, SENSORID, WorkCode FROM CHECKINOUT";
                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var allRows = new List<(string userId, DateTime checkTime, string checkType, string verifyCode, string sensorId, string workCode)>();
                    while (await reader.ReadAsync())
                    {
                        var userId = reader["USERID"].ToString();
                        var checkTime = reader["CHECKTIME"] != DBNull.Value ? Convert.ToDateTime(reader["CHECKTIME"]) : DateTime.Now;
                        var checkType = reader["CHECKTYPE"].ToString();
                        var verifyCode = reader["VERIFYCODE"].ToString();
                        var sensorId = reader["SENSORID"].ToString();
                        var workCode = reader["WorkCode"].ToString();
                        allRows.Add((userId, checkTime, checkType, verifyCode, sensorId, workCode));
                    }
                    total = allRows.Count;
                    if (progressKey != null) SyncTotal[progressKey] = total;
                    int processed = 0;
                    foreach (var row in allRows)
                    {
                        if (!employees.TryGetValue(row.userId, out var employee)) continue;
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