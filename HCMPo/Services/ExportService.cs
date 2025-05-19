using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using HCMPo.Models;

namespace HCMPo.Services
{
    public interface IExportService
    {
        Task<byte[]> GeneratePayrollPdf(Payroll payroll);
        Task<byte[]> GenerateAttendanceReportPdf(DateTime startDate, DateTime endDate, List<Attendance> attendances);
    }

    public class ExportService : IExportService
    {
        public async Task<byte[]> GeneratePayrollPdf(Payroll payroll)
        {
            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, ms);

                document.Open();

                // Add company logo
                var logo = Image.GetInstance("wwwroot/images/logo.png");
                logo.ScaleToFit(100f, 100f);
                document.Add(logo);

                // Add title
                var title = new Paragraph("Payroll Statement", new Font(Font.HELVETICA, 18, Font.BOLD));
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                // Add employee info
                document.Add(new Paragraph($"Employee: {payroll.Employee.FirstName} {payroll.Employee.LastName}"));
                document.Add(new Paragraph($"Period: {payroll.PayPeriodStartEt} - {payroll.PayPeriodEndEt}"));
                document.Add(new Paragraph(""));

                // Create salary table
                var table = new PdfPTable(2);
                table.WidthPercentage = 100;

                // Add headers
                table.AddCell(new PdfPCell(new Phrase("Description", new Font(Font.HELVETICA, 12, Font.BOLD))));
                table.AddCell(new PdfPCell(new Phrase("Amount", new Font(Font.HELVETICA, 12, Font.BOLD))));

                // Add rows
                AddPayrollRow(table, "Basic Salary", payroll.BasicSalary);
                AddPayrollRow(table, "Transport Allowance", payroll.TransportAllowance);
                AddPayrollRow(table, "Housing Allowance", payroll.HousingAllowance);
                AddPayrollRow(table, "Other Allowances", payroll.OtherAllowances);
                AddPayrollRow(table, "Gross Salary", payroll.GrossSalary);
                AddPayrollRow(table, "Income Tax", -payroll.IncomeTax);
                AddPayrollRow(table, "Pension", -payroll.PensionDeduction);
                AddPayrollRow(table, "Other Deductions", -payroll.OtherDeductions);
                AddPayrollRow(table, "Net Salary", payroll.NetSalary);

                document.Add(table);

                // Add footer
                document.Add(new Paragraph($"\nGenerated on: {DateTime.Now:dd/MM/yyyy HH:mm}"));

                document.Close();
                return ms.ToArray();
            }
        }

        public async Task<byte[]> GenerateAttendanceReportPdf(DateTime startDate, DateTime endDate, List<Attendance> attendances)
        {
            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4.Rotate(), 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, ms);

                document.Open();

                // Add title
                var title = new Paragraph("Attendance Report", new Font(Font.HELVETICA, 18, Font.BOLD));
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                // Add date range
                document.Add(new Paragraph($"Period: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}"));
                document.Add(new Paragraph(""));

                // Create attendance table
                var table = new PdfPTable(7);
                table.WidthPercentage = 100;

                // Add headers
                string[] headers = { "Date", "Employee", "Check In", "Check Out", "Duration", "Status", "Remarks" };
                foreach (var header in headers)
                {
                    table.AddCell(new PdfPCell(new Phrase(header, new Font(Font.HELVETICA, 12, Font.BOLD))));
                }

                // Add rows
                foreach (var attendance in attendances)
                {
                    table.AddCell(attendance.CheckInTime.ToString("dd/MM/yyyy"));
                    table.AddCell($"{attendance.Employee.FirstName} {attendance.Employee.LastName}");
                    table.AddCell(attendance.CheckInTime.ToString("HH:mm"));
                    table.AddCell(attendance.CheckOutTime?.ToString("HH:mm") ?? "-");
                    table.AddCell(attendance.WorkDuration?.ToString(@"hh\:mm") ?? "-");
                    table.AddCell(attendance.Status.ToString());
                    table.AddCell(attendance.StatusReason ?? "-");
                }

                document.Add(table);
                document.Close();
                return ms.ToArray();
            }
        }

        private void AddPayrollRow(PdfPTable table, string description, decimal amount)
        {
            table.AddCell(new PdfPCell(new Phrase(description)));
            table.AddCell(new PdfPCell(new Phrase(amount.ToString("N2"))));
        }
    }
} 