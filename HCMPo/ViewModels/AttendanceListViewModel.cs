using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HCMPo.ViewModels
{
    public class DailyAttendanceSlotViewModel
    {
        public DateTime Date { get; set; }
        public List<DateTime> MorningCheckIns { get; set; } = new();
        public List<DateTime> AfternoonCheckIns { get; set; } = new();
        public List<DateTime> EveningCheckOuts { get; set; } = new();
        public string Status { get; set; } = "";
        public string Remark { get; set; } = "";
        public string? EmployeeName { get; set; }
        public string? MorningStatus { get; set; }
        public string? MorningStatusReason { get; set; }
        public string? AfternoonStatus { get; set; }
        public string? AfternoonStatusReason { get; set; }
        public string? EveningStatus { get; set; }
        public string? EveningStatusReason { get; set; }
    }

    public class AttendanceListViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? EmployeeId { get; set; }
        public string? OrganizationUnitId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public IEnumerable<HCMPo.Models.Attendance> Attendances { get; set; } = new List<HCMPo.Models.Attendance>();
        public IEnumerable<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
        public IEnumerable<DailyAttendanceSlotViewModel> DailyAttendanceSlots { get; set; } = new List<DailyAttendanceSlotViewModel>();
    }
} 