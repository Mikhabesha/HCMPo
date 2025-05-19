using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HCMPo.Models;
using HCMPo.ViewModels;
using HCMPo.Data;

namespace HCMPo.Services
{
    public class AttendanceSummaryService
    {
        private readonly ApplicationDbContext _context;
        private readonly TaxCalculationService _taxCalculationService;

        public AttendanceSummaryService(ApplicationDbContext context, TaxCalculationService taxCalculationService)
        {
            _context = context;
            _taxCalculationService = taxCalculationService;
        }

        public async Task<List<EmployeeAttendanceSummaryViewModel>> GetAttendanceSummariesAsync(DateTime startDate, DateTime endDate, List<string> employeeIds)
        {
            int totalDaysInPeriod = (endDate.Date - startDate.Date).Days + 1;
            var allEmployees = await _context.Employees.Where(e => employeeIds.Contains(e.Id)).ToListAsync();
            var attendancesInRange = await _context.Attendances
                .Where(a => employeeIds.Contains(a.EmployeeId) && a.CheckInTime >= startDate && a.CheckInTime < endDate.AddDays(1))
                .ToListAsync();
            var employees = allEmployees.ToDictionary(e => e.Id);
            var summaryResults = new List<EmployeeAttendanceSummaryViewModel>();
            foreach (var empId in employeeIds)
            {
                if (!employees.TryGetValue(empId, out var employee))
                    continue;
                var empAttendances = attendancesInRange.Where(a => a.EmployeeId == empId).ToList();
                var attendancesByDay = empAttendances
                    .Where(a => a.CheckInTime.Date >= startDate.Date && a.CheckInTime.Date <= endDate.Date)
                    .GroupBy(a => a.CheckInTime.Date)
                    .ToList();
                double presentDays = 0;
                int totalLateMinutes = 0;
                int totalHalfDays = 0;
                foreach (var dayGroup in attendancesByDay)
                {
                    var dayRecords = dayGroup.ToList();
                    int presentSlots = 0;
                    if (dayRecords.Any(r => r.CheckInTime.TimeOfDay >= new TimeSpan(7, 0, 0) && r.CheckInTime.TimeOfDay <= new TimeSpan(8, 30, 0))) presentSlots++;
                    if (dayRecords.Any(r => r.CheckInTime.TimeOfDay >= new TimeSpan(12, 15, 0) && r.CheckInTime.TimeOfDay <= new TimeSpan(13, 45, 0))) presentSlots++;
                    if (dayRecords.Any(r => r.CheckInTime.TimeOfDay >= new TimeSpan(15, 45, 0) && r.CheckInTime.TimeOfDay <= new TimeSpan(17, 15, 0))) presentSlots++;
                    if (presentSlots == 3)
                        presentDays += 1;
                    else if (presentSlots > 0)
                    {
                        presentDays += 0.5;
                        totalHalfDays++;
                    }
                    var morningCheckIn = dayRecords
                        .Where(r => r.CheckInTime.TimeOfDay >= new TimeSpan(7, 0, 0) && r.CheckInTime.TimeOfDay <= new TimeSpan(8, 30, 0))
                        .OrderBy(r => r.CheckInTime)
                        .FirstOrDefault();
                    if (morningCheckIn != null && morningCheckIn.CheckInTime.TimeOfDay > new TimeSpan(8, 0, 0))
                    {
                        totalLateMinutes += (int)(morningCheckIn.CheckInTime.TimeOfDay - new TimeSpan(8, 0, 0)).TotalMinutes;
                    }
                }
                int onLeave = empAttendances.Count(a => a.Status == AttendanceStatus.OnLeave);
                int onField = empAttendances.Count(a => a.Status == AttendanceStatus.OnField);
                int holiday = empAttendances.Count(a => a.Status == AttendanceStatus.Holiday);
                int lateToAbsent = totalLateMinutes / 480;
                int lateMinutesRemainder = totalLateMinutes % 480;
                presentDays -= lateToAbsent;
                if (presentDays < 0) presentDays = 0;
                double absentDays = totalDaysInPeriod - presentDays - onLeave - onField - holiday;
                if (absentDays < 0) absentDays = 0;
                var summary = new EmployeeAttendanceSummaryViewModel
                {
                    EmployeeId = empId,
                    EmployeeName = employee.FullName,
                    TotalWorkingDaysInPeriod = totalDaysInPeriod,
                    DaysPresent = presentDays,
                    DaysAbsent = absentDays,
                    DaysLate = lateMinutesRemainder,
                    DaysEarlyDeparture = empAttendances.Count(a => a.Status == AttendanceStatus.EarlyDeparture),
                    DaysOnLeave = onLeave,
                    DaysOnField = onField,
                    DaysHoliday = holiday,
                    DaysWeekend = 0,
                    TotalWorkDuration = empAttendances.Where(a => a.WorkDuration.HasValue).Aggregate(TimeSpan.Zero, (total, next) => total + next.WorkDuration.Value)
                };
                summaryResults.Add(summary);
            }
            return summaryResults;
        }

        /// <summary>
        /// Calculates the progressive tax deduction for a given gross salary using the TaxSetting table.
        /// </summary>
        public async Task<decimal> CalculateTaxDeductionAsync(decimal grossSalary, string employeeId)
        {
            // Calculate tax based on the gross salary for the period
            return await _taxCalculationService.CalculateIncomeTaxAsync(grossSalary);
        }

        /// <summary>
        /// Calculates the pension deduction for a given gross salary using the TaxSetting table.
        /// </summary>
        public async Task<decimal> CalculatePensionDeductionAsync(decimal grossSalary, string employeeId)
        {
            // Calculate pension based on the gross salary for the period
            return _taxCalculationService.CalculatePensionDeduction(grossSalary);
        }

        /// <summary>
        /// Calculates the net salary for a given gross salary using the TaxSetting table.
        /// </summary>
        public async Task<decimal> CalculateNetSalaryAsync(decimal grossSalary, string employeeId)
        {
            // Calculate net salary based on the gross salary for the period
            var result = await _taxCalculationService.CalculateNetSalaryAsync(grossSalary, 30, 30); // Assuming full month
            return result.NetSalary;
        }

        /// <summary>
        /// Calculates the daily rate for a given gross salary using the TaxSetting table.
        /// </summary>
        public async Task<decimal> CalculateDailyRateAsync(decimal grossSalary, string employeeId)
        {
            // Calculate daily rate based on the gross salary
            return await _taxCalculationService.CalculateDailyRateAsync(grossSalary);
        }

        /// <summary>
        /// Net Salary Calculation Method 1:
        /// 1. Calculate tax on basic salary
        /// 2. Calculate after-tax amount
        /// 3. Calculate daily rate from after-tax amount
        /// 4. Calculate attendance deductions from daily rate
        /// 5. Calculate final net salary
        /// </summary>
        public async Task<decimal> CalculateNetSalaryMethod1(
            decimal basicSalary, 
            double presentDays, 
            double absentDays, 
            double lateDays,
            string employeeId,
            decimal workingDays = 30m)
        {
            // 1. Calculate tax on basic salary
            var taxDeduction = await CalculateTaxDeductionAsync(basicSalary, employeeId);

            // 2. Calculate after-tax amount
            var afterTax = basicSalary - taxDeduction;

            // 3. Calculate daily rate from after-tax amount
            var dailyRate = afterTax / workingDays;

            // 4. Calculate attendance deductions from daily rate
            var absentDeduction = dailyRate * (decimal)absentDays;
            var lateDeduction = dailyRate * (decimal)lateDays;
            var attendanceDeduction = absentDeduction + lateDeduction;

            // 5. Calculate final net salary
            return afterTax - attendanceDeduction;
        }

        /// <summary>
        /// Net Salary Calculation Method 2:
        /// Similar to Method 1 but returns the full after-tax amount minus deductions
        /// </summary>
        public async Task<decimal> CalculateNetSalaryMethod2(
            decimal basicSalary, 
            double absentDays, 
            double lateDays,
            string employeeId,
            decimal workingDays = 30m)
        {
            // 1. Calculate tax on basic salary
            var taxDeduction = await CalculateTaxDeductionAsync(basicSalary, employeeId);

            // 2. Calculate after-tax amount
            var afterTax = basicSalary - taxDeduction;

            // 3. Calculate daily rate from after-tax amount
            var dailyRate = afterTax / workingDays;

            // 4. Calculate attendance deductions from daily rate
            var absentDeduction = dailyRate * (decimal)absentDays;
            var lateDeduction = dailyRate * (decimal)lateDays;
            var attendanceDeduction = absentDeduction + lateDeduction;

            // 5. Return final net salary
            return afterTax - attendanceDeduction;
        }

        public async Task<decimal> CalculateAttendanceDeductedSalaryAsync(string employeeId, DateTime startDate, DateTime endDate)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return 0;

            var summary = await GetAttendanceSummariesAsync(startDate, endDate, new List<string> { employeeId });
            if (summary == null || summary.Count == 0) return 0;

            var daysPresent = (int)(summary.FirstOrDefault()?.DaysPresent ?? 0);

            // Calculate net salary using the corrected logic
            var result = await _taxCalculationService.CalculateNetSalaryAsync(
                employee.BasicSalary,
                daysPresent
            );

            return result.NetSalary;
        }
    }
} 