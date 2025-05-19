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
        public decimal CalculateProratedGrossSalary(decimal monthlyGrossSalary, int daysWorked, int totalDaysInMonth = 30)
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
        /// Calculates net salary after all deductions, ensuring it's never negative.
        /// </summary>
        public async Task<(decimal NetSalary, bool NeedsReview)> CalculateNetSalaryAsync(
            decimal monthlyGrossSalary, 
            int daysWorked, 
            int totalDaysInMonth = 30)
        {
            // 1. Calculate prorated gross salary
            var proratedGross = CalculateProratedGrossSalary(monthlyGrossSalary, daysWorked, totalDaysInMonth);

            // 2. Calculate pension (7% of prorated gross)
            var pension = CalculatePensionDeduction(proratedGross);

            // 3. Calculate income tax on prorated gross
            var tax = await CalculateIncomeTaxAsync(proratedGross);

            // 4. Calculate net salary
            var netSalary = proratedGross - pension - tax;

            // 5. Check if net salary is negative
            var needsReview = netSalary < 0;
            if (needsReview)
            {
                netSalary = 0; // Set to 0 if negative
            }

            return (netSalary, needsReview);
        }

        /// <summary>
        /// Calculates the daily rate based on net salary.
        /// </summary>
        public async Task<decimal> CalculateDailyRateAsync(decimal grossSalary)
        {
            var result = await CalculateNetSalaryAsync(grossSalary, 30); // Assuming full month
            return result.NetSalary / 30m; // Assuming 30 days per month
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