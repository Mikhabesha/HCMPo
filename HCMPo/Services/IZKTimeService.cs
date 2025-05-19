using System.Threading.Tasks;
using HCMPo.Models;

namespace HCMPo.Services
{
    public interface IZKTimeService
    {
        Task<bool> TestConnectionAsync();
        Task<bool> SyncAttendancesAsync();
        Task<ZKTimeAttendance> GetLastAttendanceAsync();
        Task<bool> ConnectAsync();
        Task<bool> DisconnectAsync();
    }
} 