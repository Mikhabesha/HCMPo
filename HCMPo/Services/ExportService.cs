using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using HCMPo.Models;

namespace HCMPo.Services
{
    public interface IExportService
    {
        Task<byte[]> GeneratePayrollPdf(Payroll payroll);
        Task<byte[]> GeneratePayrollExcel(Payroll payroll);
        Task<byte[]> GenerateAttendanceReportPdf(DateTime startDate, DateTime endDate, List<Attendance> attendances);
        Task<byte[]> GenerateAttendanceReportExcel(DateTime startDate, DateTime endDate, List<Attendance> attendances);
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
                var title = new Paragraph("Payroll Statement", new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD));
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
                table.AddCell(new PdfPCell(new Phrase("Description", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD))));
                table.AddCell(new PdfPCell(new Phrase("Amount", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD))));

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

        public async Task<byte[]> GeneratePayrollExcel(Payroll payroll)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Payroll");

                // Add title
                worksheet.Cells[1, 1].Value = "Payroll Statement";
                worksheet.Cells[1, 1, 1, 2].Merge = true;

                // Add employee info
                worksheet.Cells[3, 1].Value = "Employee:";
                worksheet.Cells[3, 2].Value = $"{payroll.Employee.FirstName} {payroll.Employee.LastName}";
                worksheet.Cells[4, 1].Value = "Period:";
                worksheet.Cells[4, 2].Value = $"{payroll.PayPeriodStartEt} - {payroll.PayPeriodEndEt}";

                // Add headers
                worksheet.Cells[6, 1].Value = "Description";
                worksheet.Cells[6, 2].Value = "Amount";

                // Add data
                var row = 7;
                AddPayrollExcelRow(worksheet, ref row, "Basic Salary", payroll.BasicSalary);
                AddPayrollExcelRow(worksheet, ref row, "Transport Allowance", payroll.TransportAllowance);
                AddPayrollExcelRow(worksheet, ref row, "Housing Allowance", payroll.HousingAllowance);
                AddPayrollExcelRow(worksheet, ref row, "Other Allowances", payroll.OtherAllowances);
                AddPayrollExcelRow(worksheet, ref row, "Gross Salary", payroll.GrossSalary);
                AddPayrollExcelRow(worksheet, ref row, "Income Tax", -payroll.IncomeTax);
                AddPayrollExcelRow(worksheet, ref row, "Pension", -payroll.PensionDeduction);
                AddPayrollExcelRow(worksheet, ref row, "Other Deductions", -payroll.OtherDeductions);
                AddPayrollExcelRow(worksheet, ref row, "Net Salary", payroll.NetSalary);

                // Style the worksheet
                worksheet.Column(1).AutoFit();
                worksheet.Column(2).AutoFit();

                return await package.GetAsByteArrayAsync();
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
                var title = new Paragraph("Attendance Report", new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD));
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
                    table.AddCell(new PdfPCell(new Phrase(header, new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD))));
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

        public async Task<byte[]> GenerateAttendanceReportExcel(DateTime startDate, DateTime endDate, List<Attendance> attendances)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Attendance");

                // Add title
                worksheet.Cells[1, 1].Value = "Attendance Report";
                worksheet.Cells[1, 1, 1, 7].Merge = true;

                // Add date range
                worksheet.Cells[2, 1].Value = $"Period: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}";
                worksheet.Cells[2, 1, 2, 7].Merge = true;

                // Add headers
                string[] headers = { "Date", "Employee", "Check In", "Check Out", "Duration", "Status", "Remarks" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[4, i + 1].Value = headers[i];
                }

                // Add data
                var row = 5;
                foreach (var attendance in attendances)
                {
                    worksheet.Cells[row, 1].Value = attendance.CheckInTime.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 2].Value = $"{attendance.Employee.FirstName} {attendance.Employee.LastName}";
                    worksheet.Cells[row, 3].Value = attendance.CheckInTime.ToString("HH:mm");
                    worksheet.Cells[row, 4].Value = attendance.CheckOutTime?.ToString("HH:mm") ?? "-";
                    worksheet.Cells[row, 5].Value = attendance.WorkDuration?.ToString(@"hh\:mm") ?? "-";
                    worksheet.Cells[row, 6].Value = attendance.Status.ToString();
                    worksheet.Cells[row, 7].Value = attendance.StatusReason ?? "-";

                    // Color code the status
                    var statusCell = worksheet.Cells[row, 6];
                    switch (attendance.Status)
                    {
                        case AttendanceStatus.Present:
                            statusCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            statusCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                            break;
                        case AttendanceStatus.Absent:
                            statusCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            statusCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightPink);
                            break;
                        case AttendanceStatus.Late:
                            statusCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            statusCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
                            break;
                    }

                    row++;
                }

                // Style the worksheet
                for (int i = 1; i <= 7; i++)
                {
                    worksheet.Column(i).AutoFit();
                }

                return await package.GetAsByteArrayAsync();
            }
        }

        private void AddPayrollRow(PdfPTable table, string description, decimal amount)
        {
            table.AddCell(new PdfPCell(new Phrase(description)));
            table.AddCell(new PdfPCell(new Phrase(amount.ToString("N2"))));
        }

        private void AddPayrollExcelRow(ExcelWorksheet worksheet, ref int row, string description, decimal amount)
        {
            worksheet.Cells[row, 1].Value = description;
            worksheet.Cells[row, 2].Value = amount;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
            row++;
        }
    }
} 