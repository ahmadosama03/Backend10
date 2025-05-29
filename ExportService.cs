using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SDMS.Domain.Entities;
using SDMS.Core.DTOs;
using System.Linq;
using SDMS.Infrastructure.Data;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using iText.Kernel.Font; // Added for font factory
using iText.IO.Font.Constants; // Added for standard fonts

namespace SDMS.Core.Services
{
    public class ExportService
    {
        private readonly ApplicationDbContext _context;
        private readonly PdfFont _boldFont;
        private readonly PdfFont _italicFont;

        public ExportService(ApplicationDbContext context)
        {
            _context = context;
            // Set EPPlus license context (adjust if using a commercial license)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; 
            // Pre-create fonts for efficiency
            _boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            _italicFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);
        }

        // --- Financial Report Exports --- 

        public async Task<string> ExportFinancialReportToPdfAsync(int startupId, DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddMonths(-3);
            endDate ??= DateTime.UtcNow;

            var startup = await _context.Startups.FindAsync(startupId);
            if (startup == null)
                throw new ArgumentException("Startup not found");

            var metrics = await _context.FinancialMetrics
                .Where(fm => fm.StartupId == startupId && 
                       fm.Date >= startDate && 
                       fm.Date <= endDate)
                .OrderBy(fm => fm.Date)
                .ToListAsync();

            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            Directory.CreateDirectory(directory); // Ensure directory exists
            var fileName = $"Financial_Report_{startup.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(directory, fileName);

            using (var writer = new PdfWriter(filePath))
            using (var pdf = new PdfDocument(writer))
            using (var document = new Document(pdf))
            {
                // Title
                document.Add(new Paragraph($"Financial Report - {startup.Name}")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(20).SetFont(_boldFont)); 
                // Period
                document.Add(new Paragraph($"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(12));
                document.Add(new Paragraph("\n"));

                // Summary Section
                document.Add(new Paragraph("Financial Summary").SetFontSize(16).SetFont(_boldFont)); 
                var totalRevenue = metrics.Where(m => m.MetricType == "Revenue").Sum(m => m.Value);
                var totalExpenses = metrics.Where(m => m.MetricType == "Expense").Sum(m => m.Value);
                var totalSales = metrics.Where(m => m.MetricType == "SalesUnits").Sum(m => m.Value); 
                var netProfit = totalRevenue - totalExpenses;

                var summaryTable = new Table(2).UseAllAvailableWidth();
                summaryTable.AddCell(new Cell().Add(new Paragraph("Total Revenue").SetFont(_boldFont))); 
                summaryTable.AddCell(new Cell().Add(new Paragraph($"{totalRevenue:C2}"))); 
                summaryTable.AddCell(new Cell().Add(new Paragraph("Total Expenses").SetFont(_boldFont))); 
                summaryTable.AddCell(new Cell().Add(new Paragraph($"{totalExpenses:C2}")));
                summaryTable.AddCell(new Cell().Add(new Paragraph("Total Sales (Units)").SetFont(_boldFont))); 
                summaryTable.AddCell(new Cell().Add(new Paragraph($"{totalSales:N0} units")));
                summaryTable.AddCell(new Cell().Add(new Paragraph("Net Profit").SetFont(_boldFont))); 
                summaryTable.AddCell(new Cell().Add(new Paragraph($"{netProfit:C2}")));
                document.Add(summaryTable);
                document.Add(new Paragraph("\n"));

                // Detailed Metrics Section
                document.Add(new Paragraph("Detailed Financial Metrics").SetFontSize(16).SetFont(_boldFont)); 
                if (metrics.Any())
                {
                    var metricsByDate = metrics.GroupBy(m => m.Date.Date).OrderBy(g => g.Key);
                    var table = new Table(5).UseAllAvailableWidth();
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Date").SetFont(_boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Revenue").SetFont(_boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Expenses").SetFont(_boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Sales (Units)").SetFont(_boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Profit").SetFont(_boldFont)));

                    foreach (var dateGroup in metricsByDate)
                    {
                        var date = dateGroup.Key;
                        var dailyRevenue = dateGroup.FirstOrDefault(m => m.MetricType == "Revenue")?.Value ?? 0;
                        var dailyExpenses = dateGroup.FirstOrDefault(m => m.MetricType == "Expense")?.Value ?? 0;
                        var dailySales = dateGroup.FirstOrDefault(m => m.MetricType == "SalesUnits")?.Value ?? 0;
                        var dailyProfit = dailyRevenue - dailyExpenses;

                        table.AddCell(date.ToString("yyyy-MM-dd"));
                        table.AddCell($"{dailyRevenue:C2}");
                        table.AddCell($"{dailyExpenses:C2}");
                        table.AddCell($"{dailySales:N0}");
                        table.AddCell($"{dailyProfit:C2}");
                    }
                    document.Add(table);
                }
                else
                {
                    document.Add(new Paragraph("No detailed data available for the selected period.").SetTextAlignment(TextAlignment.CENTER).SetFontSize(12).SetFont(_italicFont)); // Use SetFont for italic
                }

                // Notes Section
                document.Add(new Paragraph("\n"));
                document.Add(new Paragraph("Notes").SetFontSize(16).SetFont(_boldFont)); 
                var notes = metrics.Where(m => !string.IsNullOrEmpty(m.Notes)).ToList();
                if (notes.Any())
                {
                    foreach (var note in notes)
                    {
                        document.Add(new Paragraph($"{note.Date:yyyy-MM-dd} ({note.MetricType}): {note.Notes}"));
                    }
                }
                else
                {
                    document.Add(new Paragraph("No notes available.").SetFontSize(12).SetFont(_italicFont)); // Use SetFont for italic
                }

                // Footer
                document.Add(new Paragraph("\n\n"));
                document.Add(new Paragraph($"Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC")
                    .SetTextAlignment(TextAlignment.RIGHT).SetFontSize(10).SetFont(_italicFont)); // Use SetFont for italic
            }
            return filePath;
        }

        public async Task<string> ExportFinancialReportToExcelAsync(int startupId, DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddMonths(-3);
            endDate ??= DateTime.UtcNow;

            var startup = await _context.Startups.FindAsync(startupId);
            if (startup == null)
                throw new ArgumentException("Startup not found");

            var metrics = await _context.FinancialMetrics
                .Where(fm => fm.StartupId == startupId && 
                       fm.Date >= startDate && 
                       fm.Date <= endDate)
                .OrderBy(fm => fm.Date)
                .ToListAsync();

            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            Directory.CreateDirectory(directory); // Ensure directory exists
            var fileName = $"Financial_Report_{startup.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(directory, fileName);

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                // Summary Worksheet
                var summaryWs = package.Workbook.Worksheets.Add("Summary");
                summaryWs.Cells["A1"].Value = $"Financial Report - {startup.Name}";
                summaryWs.Cells["A1:E1"].Merge = true;
                summaryWs.Cells["A1"].Style.Font.Size = 20;
                summaryWs.Cells["A1"].Style.Font.Bold = true;
                summaryWs.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                summaryWs.Cells["A2"].Value = $"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}";
                summaryWs.Cells["A2:E2"].Merge = true;
                summaryWs.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                summaryWs.Cells["A4"].Value = "Financial Summary";
                summaryWs.Cells["A4"].Style.Font.Bold = true;
                summaryWs.Cells["A4"].Style.Font.Size = 16;

                var totalRevenue = metrics.Where(m => m.MetricType == "Revenue").Sum(m => m.Value);
                var totalExpenses = metrics.Where(m => m.MetricType == "Expense").Sum(m => m.Value);
                var totalSales = metrics.Where(m => m.MetricType == "SalesUnits").Sum(m => m.Value);
                var netProfit = totalRevenue - totalExpenses;

                summaryWs.Cells["A5"].Value = "Total Revenue"; summaryWs.Cells["A5"].Style.Font.Bold = true;
                summaryWs.Cells["B5"].Value = totalRevenue; summaryWs.Cells["B5"].Style.Numberformat.Format = "$#,##0.00";
                summaryWs.Cells["A6"].Value = "Total Expenses"; summaryWs.Cells["A6"].Style.Font.Bold = true;
                summaryWs.Cells["B6"].Value = totalExpenses; summaryWs.Cells["B6"].Style.Numberformat.Format = "$#,##0.00";
                summaryWs.Cells["A7"].Value = "Total Sales (Units)"; summaryWs.Cells["A7"].Style.Font.Bold = true;
                summaryWs.Cells["B7"].Value = totalSales; summaryWs.Cells["B7"].Style.Numberformat.Format = "#,##0";
                summaryWs.Cells["A8"].Value = "Net Profit"; summaryWs.Cells["A8"].Style.Font.Bold = true;
                summaryWs.Cells["B8"].Value = netProfit; summaryWs.Cells["B8"].Style.Numberformat.Format = "$#,##0.00";
                summaryWs.Column(1).AutoFit();
                summaryWs.Column(2).AutoFit();

                // Details Worksheet
                var detailsWs = package.Workbook.Worksheets.Add("Details");
                detailsWs.Cells["A1"].Value = "Date";
                detailsWs.Cells["B1"].Value = "Metric Type";
                detailsWs.Cells["C1"].Value = "Value";
                detailsWs.Cells["D1"].Value = "Period";
                detailsWs.Cells["E1"].Value = "Notes";
                detailsWs.Cells["A1:E1"].Style.Font.Bold = true;

                if (metrics.Any())
                {
                    detailsWs.Cells["A2"].LoadFromCollection(metrics.Select(m => new {
                        m.Date,
                        m.MetricType,
                        m.Value,
                        m.Period,
                        m.Notes
                    }), false); // Load data starting from A2, without headers

                    detailsWs.Column(1).Style.Numberformat.Format = "yyyy-mm-dd";
                    detailsWs.Column(3).Style.Numberformat.Format = "$#,##0.00"; // Assuming most values are currency
                }
                detailsWs.Cells[detailsWs.Dimension.Address].AutoFitColumns();

                await package.SaveAsync();
            }
            return filePath;
        }

        // --- Employee Report Exports (NEW) ---

        public async Task<string> ExportEmployeeReportToPdfAsync(int startupId)
        {
            var startup = await _context.Startups
                .Include(s => s.Employees)
                    .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(s => s.Id == startupId);

            if (startup == null)
                throw new ArgumentException("Startup not found");

            var employees = startup.Employees.ToList();

            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            Directory.CreateDirectory(directory); // Ensure directory exists
            var fileName = $"Employee_Report_{startup.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(directory, fileName);

            using (var writer = new PdfWriter(filePath))
            using (var pdf = new PdfDocument(writer))
            using (var document = new Document(pdf))
            {
                // Title
                document.Add(new Paragraph($"Employee Report - {startup.Name}")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(20).SetFont(_boldFont)); 
                document.Add(new Paragraph($"Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(12));
                document.Add(new Paragraph("\n"));

                // Employee Table
                if (employees.Any())
                {
                    var table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 3, 3, 3, 2, 2 })).UseAllAvailableWidth();
                    table.AddHeaderCell(new Cell().Add(new Paragraph("ID").SetFont(_boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Name").SetFont(_boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Email").SetFont(_boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Position").SetFont(_boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Hire Date").SetFont(_boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Status").SetFont(_boldFont)));

                    foreach (var emp in employees)
                    {
                        table.AddCell(emp.UserId.ToString());
                        table.AddCell(emp.User?.Name ?? "N/A");
                        table.AddCell(emp.User?.Email ?? "N/A");
                        table.AddCell(emp.Position ?? "N/A"); 
                        table.AddCell(emp.HireDate?.ToString("yyyy-MM-dd") ?? "N/A"); 
                        table.AddCell(emp.User?.IsActive == true ? "Active" : "Inactive");
                    }
                    document.Add(table);
                }
                else
                {
                    document.Add(new Paragraph("No employees found for this startup.").SetTextAlignment(TextAlignment.CENTER).SetFontSize(12).SetFont(_italicFont)); // Use SetFont for italic
                }
            }
            return filePath;
        }

        public async Task<string> ExportEmployeeReportToExcelAsync(int startupId)
        {
            var startup = await _context.Startups
                .Include(s => s.Employees)
                    .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(s => s.Id == startupId);

            if (startup == null)
                throw new ArgumentException("Startup not found");

            var employees = startup.Employees.ToList();

            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            Directory.CreateDirectory(directory); // Ensure directory exists
            var fileName = $"Employee_Report_{startup.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(directory, fileName);

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var ws = package.Workbook.Worksheets.Add("Employees");
                ws.Cells["A1"].Value = $"Employee Report - {startup.Name}";
                ws.Cells["A1:F1"].Merge = true;
                ws.Cells["A1"].Style.Font.Size = 20;
                ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A2"].Value = $"Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
                ws.Cells["A2:F2"].Merge = true;
                ws.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Headers
                ws.Cells["A4"].Value = "ID";
                ws.Cells["B4"].Value = "Name";
                ws.Cells["C4"].Value = "Email";
                ws.Cells["D4"].Value = "Position";
                ws.Cells["E4"].Value = "Hire Date";
                ws.Cells["F4"].Value = "Status";
                ws.Cells["A4:F4"].Style.Font.Bold = true;

                if (employees.Any())
                {
                    ws.Cells["A5"].LoadFromCollection(employees.Select(emp => new {
                        Id = emp.UserId,
                        Name = emp.User?.Name ?? "N/A",
                        Email = emp.User?.Email ?? "N/A",
                        Position = emp.Position ?? "N/A", 
                        HireDate = emp.HireDate, 
                        Status = emp.User?.IsActive == true ? "Active" : "Inactive"
                    }), false); // Load data starting from A5, without headers

                    ws.Column(5).Style.Numberformat.Format = "yyyy-mm-dd"; // Format Hire Date column
                }
                ws.Cells[ws.Dimension.Address].AutoFitColumns();

                await package.SaveAsync();
            }
            return filePath;
        }
    }
}

