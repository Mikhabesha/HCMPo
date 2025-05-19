using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HCMPo.Models;

namespace HCMPo.Services
{
    public interface IAttendanceSyncService
    {
        Task SyncAttendanceDataAsync();
        Task<List<ZKTimeAttendance>> GetRawAttendanceDataAsync(DateTime? fromDate = null);
        Task<int> SyncEmployeesFromAttDbAsync(string progressKey = null);
        Task<int> SyncAttendanceFromAttDbAsync(string progressKey = null);
        int GetSyncProgress(string key);
        int GetSyncTotal(string key);
    }
} 