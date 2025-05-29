using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HCMPo.Data;
using HCMPo.Models;

namespace HCMPo.Services
{
    public class TaxCalculationService
    {
        private readonly ApplicationDbContext _context;

        public TaxCalculationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calculates the prorated gross salary based on days worked
        /// </summary>
        public decimal CalculateProratedGrossSalary(decimal monthlyGrossSalary, decimal daysWorked, int totalDaysInMonth = 30)
        {
            return (monthlyGrossSalary / totalDaysInMonth) * daysWorked;
        }

        /// <summary>
        /// Calculates the progressive tax deduction for a given prorated gross salary.
        /// </summary>
        public async Task<decimal> CalculateIncomeTaxAsync(decimal proratedGrossSalary)
        {
            // Get tax settings from database
            var taxSettings = await _context.TaxSettings
                .Where(t => t.Type == TaxType.IncomeTax && t.IsActive)
                .OrderBy(t => t.MinSalary)
                .ToListAsync();

            // Find applicable tax bracket
            var applicableBracket = taxSettings
                .Where(b => proratedGrossSalary >= b.MinSalary && proratedGrossSalary <= b.MaxSalary)
                .FirstOrDefault();

            if (applicableBracket == null)
            {
                // If salary is above highest bracket, use the highest bracket
                applicableBracket = taxSettings.OrderByDescending(b => b.MaxSalary).First();
            }

            // Calculate tax using the bracket's formula
            var tax = (proratedGrossSalary * (applicableBracket.Percentage / 100m)) - (applicableBracket.Subtraction ?? 0);

            return Math.Max(0, tax); // Ensure tax is not negative
        }

        /// <summary>
        /// Calculates pension deduction (fixed at 7% of prorated gross salary).
        /// </summary>
        public decimal CalculatePensionDeduction(decimal proratedGrossSalary)
        {
            return proratedGrossSalary * 0.07m;
        }

        /// <summary>
        /// Calculates other deductions for an employee based on their EmployeeTax records
        /// </summary>
        public async Task<decimal> CalculateOtherDeductionsAsync(string employeeId, decimal proratedGrossSalary)
        {
            // Join EmployeeTaxes with DeductionTypes to check both IsApplied and IsActive
            var employeeTaxes = await (from et in _context.EmployeeTaxes
                                       join dt in _context.DeductionTypes on et.TaxName equals dt.DisplayName
                                       where et.EmployeeId == employeeId
                                             && et.IsApplied == true
                                             && dt.IsActive == true
                                             && et.TaxName != "Income Tax"
                                             && et.TaxName != "Pension"
                                       select et).ToListAsync();

            decimal totalDeductions = 0;
            foreach (var tax in employeeTaxes)
            {
                totalDeductions += tax.Percentage;
            }

            return totalDeductions;
        }

        /// <summary>
        /// Calculates the net salary for a given gross salary and employee ID
        /// </summary>
        public async Task<decimal> CalculateNetSalaryAsync(decimal proratedGrossSalary, string employeeId)
        {
            var incomeTax = await CalculateIncomeTaxAsync(proratedGrossSalary);
            var pensionDeduction = CalculatePensionDeduction(proratedGrossSalary);
            var otherDeductions = await CalculateOtherDeductionsAsync(employeeId, proratedGrossSalary);

            return proratedGrossSalary - incomeTax - pensionDeduction - otherDeductions;
        }

        /// <summary>
        /// Calculates the daily rate based on net salary.
        /// </summary>
        public async Task<decimal> CalculateDailyRateAsync(decimal grossSalary, string employeeId)
        {
            var netSalary = await CalculateNetSalaryAsync(grossSalary, employeeId);
            return netSalary / 30m; // Assuming 30 days per month
        }

        public async Task<decimal> CalculateAttendanceDeductionAsync(decimal grossSalary, int daysAbsent, int totalDaysInMonth = 30)
        {
            // Calculate daily rate
            var dailyRate = grossSalary / totalDaysInMonth;
            
            // Calculate attendance deduction
            return dailyRate * daysAbsent;
        }
    }
} 