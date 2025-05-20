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
using System.Text;



namespace SDMS.Core.Services
{
    public class ExportService
    {
        private readonly ApplicationDbContext _context;

        public ExportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> ExportFinancialReportToPdfAsync(int startupId, DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddMonths(-3);
            endDate ??= DateTime.UtcNow;

            var startup = await _context.Startups.FindAsync(startupId);
            if (startup == null)
                throw new ArgumentException("Startup not found");

            var metrics = await _context.FinancialMetrics
                .Where(fm => fm.StartupId == startupId && 
                       !fm.IsArchived && 
                       fm.Date >= startDate && 
                       fm.Date <= endDate)
                .OrderBy(fm => fm.Date)
                .ToListAsync();

            // Create directory if it doesn't exist
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Create file path
            var fileName = $"Financial_Report_{startup.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(directory, fileName);

            using (var writer = new PdfWriter(filePath))
            {
                using (var pdf = new PdfDocument(writer))
                {
                    using (var document = new Document(pdf))
                    {
                        // Add title
                        document.Add(new Paragraph($"Financial Report - {startup.Name}")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(20)
                            .SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD)));

                        // Add report period
                        document.Add(new Paragraph($"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(12));

                        document.Add(new Paragraph("\n"));

                        // Add summary
                        document.Add(new Paragraph("Financial Summary")
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetFontSize(16)
                            .SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD)));

                        var totalRevenue = metrics.Sum(m => m.Revenue);
                        var totalExpenses = metrics.Sum(m => m.Expenses);
                        var totalSales = metrics.Sum(m => m.MonthlySales);
                        var netProfit = totalRevenue - totalExpenses;

                        // Create summary table
                        var summaryTable = new Table(2).UseAllAvailableWidth();
                        
                        summaryTable.AddCell(new Cell().Add(new Paragraph("Total Revenue").SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))));
                        summaryTable.AddCell(new Cell().Add(new Paragraph($"${totalRevenue:N2}")));
                        
                        summaryTable.AddCell(new Cell().Add(new Paragraph("Total Expenses").SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))));
                        summaryTable.AddCell(new Cell().Add(new Paragraph($"${totalExpenses:N2}")));
                        
                        summaryTable.AddCell(new Cell().Add(new Paragraph("Total Sales").SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))));
                        summaryTable.AddCell(new Cell().Add(new Paragraph($"{totalSales:N0} units")));
                        
                        summaryTable.AddCell(new Cell().Add(new Paragraph("Net Profit").SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))));
                        summaryTable.AddCell(new Cell().Add(new Paragraph($"${netProfit:N2}")));

                        document.Add(summaryTable);
                        document.Add(new Paragraph("\n"));

                        // Add detailed metrics
                        document.Add(new Paragraph("Detailed Financial Metrics")
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetFontSize(16)
                            .SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD)));

                        if (metrics.Any())
                        {
                            // Create metrics table
                            var table = new Table(5).UseAllAvailableWidth();
                            
                            // Add headers
                            table.AddCell(new Cell().Add(new Paragraph("Date").SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))));
                            table.AddCell(new Cell().Add(new Paragraph("Revenue").SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))));
                            table.AddCell(new Cell().Add(new Paragraph("Expenses").SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))));
                            table.AddCell(new Cell().Add(new Paragraph("Sales").SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))));
                            table.AddCell(new Cell().Add(new Paragraph("Profit").SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))));
                            
                            // Add data rows
                            foreach (var metric in metrics)
                            {
                                table.AddCell(new Cell().Add(new Paragraph(metric.Date.ToString("yyyy-MM-dd"))));
                                table.AddCell(new Cell().Add(new Paragraph($"${metric.Revenue:N2}")));
                                table.AddCell(new Cell().Add(new Paragraph($"${metric.Expenses:N2}")));
                                table.AddCell(new Cell().Add(new Paragraph($"{metric.MonthlySales:N0}")));
                                table.AddCell(new Cell().Add(new Paragraph($"${metric.Revenue - metric.Expenses:N2}")));
                            }
                            
                            document.Add(table);
                        }
                        else
                        {
                            document.Add(new Paragraph("No data available.")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(12)
                               // .SetItalic()
                                );
                        }

                        // Add notes
                        document.Add(new Paragraph("\n"));
                        document.Add(new Paragraph("Notes")
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetFontSize(16)
                            .SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD)));
                        
                        var notes = metrics.Where(m => !string.IsNullOrEmpty(m.Notes)).ToList();
                        if (notes.Any())
                        {
                            foreach (var note in notes)
                            {
                                document.Add(new Paragraph($"{note.Date:yyyy-MM-dd}: {note.Notes}"));
                            }
                        }
                        else
                        {
                            document.Add(new Paragraph("No notes available.")
                                .SetTextAlignment(TextAlignment.LEFT)
                                .SetFontSize(12)
                               // .ic()
                                );
                        }

                        // Add footer
                        document.Add(new Paragraph("\n\n"));
                        document.Add(new Paragraph($"Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC")
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetFontSize(10)
                           // .SetItalic()
                            );
                    }
                }
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
                       !fm.IsArchived && 
                       fm.Date >= startDate && 
                       fm.Date <= endDate)
                .OrderBy(fm => fm.Date)
                .ToListAsync();

            // Create directory if it doesn't exist
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Create file path
            var fileName = $"Financial_Report_{startup.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(directory, fileName);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                // Create the Summary worksheet
                var summaryWorksheet = package.Workbook.Worksheets.Add("Summary");
                
                // Add title
                summaryWorksheet.Cells[1, 1].Value = $"Financial Report - {startup.Name}";
                summaryWorksheet.Cells[1, 1, 1, 5].Merge = true;
                summaryWorksheet.Cells[1, 1].Style.Font.Size = 20;
                summaryWorksheet.Cells[1, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                
                // Add report period
                summaryWorksheet.Cells[2, 1].Value = $"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}";
                summaryWorksheet.Cells[2, 1, 2, 5].Merge = true;
                summaryWorksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                
                // Add summary
                summaryWorksheet.Cells[4, 1].Value = "Financial Summary";
                summaryWorksheet.Cells[4, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[4, 1].Style.Font.Size = 16;
                
                var totalRevenue = metrics.Sum(m => m.Revenue);
                var totalExpenses = metrics.Sum(m => m.Expenses);
                var totalSales = metrics.Sum(m => m.MonthlySales);
                var netProfit = totalRevenue - totalExpenses;
                
                summaryWorksheet.Cells[5, 1].Value = "Total Revenue";
                summaryWorksheet.Cells[5, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[5, 2].Value = totalRevenue;
                summaryWorksheet.Cells[5, 2].Style.Numberformat.Format = "$#,##0.00";
                
                summaryWorksheet.Cells[6, 1].Value = "Total Expenses";
                summaryWorksheet.Cells[6, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[6, 2].Value = totalExpenses;
                summaryWorksheet.Cells[6, 2].Style.Numberformat.Format = "$#,##0.00";
                
                summaryWorksheet.Cells[7, 1].Value = "Total Sales";
                summaryWorksheet.Cells[7, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[7, 2].Value = totalSales;
                summaryWorksheet.Cells[7, 2].Style.Numberformat.Format = "#,##0";
                
                summaryWorksheet.Cells[8, 1].Value = "Net Profit";
                summaryWorksheet.Cells[8, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[8, 2].Value = netProfit;
                summaryWorksheet.Cells[8, 2].Style.Numberformat.Format = "$#,##0.00";
                
                // Create the Details worksheet
                var detailsWorksheet = package.Workbook.Worksheets.Add("Details");
                
                // Add headers
                detailsWorksheet.Cells[1, 1].Value = "Date";
                detailsWorksheet.Cells[1, 2].Value = "Revenue";
                detailsWorksheet.Cells[1, 3].Value = "Expenses";
                detailsWorksheet.Cells[1, 4].Value = "Sales";
                detailsWorksheet.Cells[1, 5].Value = "Profit";
                detailsWorksheet.Cells[1, 6].Value = "Notes";
                
                // Style headers
                for (int i = 1; i <= 6; i++)
                {
                    detailsWorksheet.Cells[1, i].Style.Font.Bold = true;
                }
                
                // Add data
                if (metrics.Any())
                {
                    for (int i = 0; i < metrics.Count; i++)
                    {
                        var metric = metrics[i];
                        int row = i + 2;
                        
                        detailsWorksheet.Cells[row, 1].Value = metric.Date;
                        detailsWorksheet.Cells[row, 1].Style.Numberformat.Format = "yyyy-mm-dd";
                        
                        detailsWorksheet.Cells[row, 2].Value = metric.Revenue;
                        detailsWorksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                        
                        detailsWorksheet.Cells[row, 3].Value = metric.Expenses;
                        detailsWorksheet.Cells[row, 3].Style.Numberformat.Format = "$#,##0.00";
                        
                        detailsWorksheet.Cells[row, 4].Value = metric.MonthlySales;
                        detailsWorksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0";
                        
                        detailsWorksheet.Cells[row, 5].Value = metric.Revenue - metric.Expenses;
                        detailsWorksheet.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";
                        
                        detailsWorksheet.Cells[row, 6].Value = metric.Notes;
                    }
                }
                else
                {
                    detailsWorksheet.Cells[2, 1].Value = "No data available.";
                    detailsWorksheet.Cells[2, 1].Style.Font.Italic = true;
                }
                
                // Auto-fit columns
                summaryWorksheet.Cells.AutoFitColumns();
                detailsWorksheet.Cells.AutoFitColumns();
                
                // Save the Excel file
                package.SaveAs(new FileInfo(filePath));
            }

            return filePath;
        }

        public async Task<string> ExportEmployeeReportToPdfAsync(int startupId)
        {
            var startup = await _context.Startups.FindAsync(startupId);
            if (startup == null)
                throw new ArgumentException("Startup not found");

            var employees = await _context.Employees
                .Include(e => e.User)
                .Where(e => e.StartupId == startupId && e.User.IsActive)
                .OrderBy(e => e.User.Username)
                .ToListAsync();

            // Create directory if it doesn't exist
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Create file path
            var fileName = $"Employee_Report_{startup.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(directory, fileName);

            using (var writer = new PdfWriter(filePath))
            {
                using (var pdf = new PdfDocument(writer))
                {
                    using (var document = new Document(pdf))
                    {
                        // Add title
                        document.Add(new Paragraph($"Employee Report - {startup.Name}")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(20)
                            .SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD)));

                        // Add report date
                        document.Add(new Paragraph($"Generated on: {DateTime.UtcNow:yyyy-MM-dd}")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(12));

                        document.Add(new Paragraph("\n"));

                        // Add summary
                        document.Add(new Paragraph("Employee Summary")
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetFontSize(16)
                            .SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD)));

                        // Create summary table
                        var summaryTable = new Table(2).UseAllAvailableWidth();
                        
                        summaryTable.AddCell(new Cell().Add(new Paragraph("Total Employees")));//setbold was deleted
                        summaryTable.AddCell(new Cell().Add(new Paragraph($"{employees.Count}")));
                        
                        var avgPerformance = employees.Any() ? employees.Average(e => e.PerformanceScore) : 0;
                        summaryTable.AddCell(new Cell().Add(new Paragraph("Average Performance Score")));//setbold was deleted
                        summaryTable.AddCell(new Cell().Add(new Paragraph($"{avgPerformance:F2}")));
                        
                        var roleDistribution = employees
                            .GroupBy(e => e.EmployeeRole)
                            .Select(g => new { Role = g.Key, Count = g.Count() })
                            .OrderByDescending(x => x.Count)
                            .ToList();
                        
                        summaryTable.AddCell(new Cell().Add(new Paragraph("Role Distribution")));//setbold was deleted

                        var roleDistributionText = new StringBuilder();
                        foreach (var role in roleDistribution)
                        {
                            roleDistributionText.AppendLine($"{role.Role}: {role.Count}");
                        }
                        summaryTable.AddCell(new Cell().Add(new Paragraph(roleDistributionText.ToString())));

                        document.Add(summaryTable);
                        document.Add(new Paragraph("\n"));

                        // Add detailed employee list
                        document.Add(new Paragraph("Employee Details")
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetFontSize(16)
                            .SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD)));

                        if (employees.Any())
                        {
                            // Create employee table
                            var table = new Table(5).UseAllAvailableWidth();
                            
                            // Add headers
                            table.AddCell(new Cell().Add(new Paragraph("Name")));//setbold was deleted
                            table.AddCell(new Cell().Add(new Paragraph("Role")));//setbold was deleted
                            table.AddCell(new Cell().Add(new Paragraph("Email")));//setbold was deleted
                            table.AddCell(new Cell().Add(new Paragraph("Hire Date")));//setbold was deleted
                            table.AddCell(new Cell().Add(new Paragraph("Performance")));//setbold was deleted
                            
                            // Add data rows
                            foreach (var employee in employees)
                            {
                                table.AddCell(new Cell().Add(new Paragraph(employee.User.Username)));
                                table.AddCell(new Cell().Add(new Paragraph(employee.EmployeeRole)));
                                table.AddCell(new Cell().Add(new Paragraph(employee.User.Email)));
                                table.AddCell(new Cell().Add(new Paragraph(employee.HireDate.ToString("yyyy-MM-dd"))));
                                table.AddCell(new Cell().Add(new Paragraph($"{employee.PerformanceScore:F2}")));
                            }
                            
                            document.Add(table);
                        }
                        else
                        {
                            document.Add(new Paragraph("No employees available.")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(12)
                              //  .SetItalic()
                                );
                        }

                        // Add footer
                        document.Add(new Paragraph("\n\n"));
                        document.Add(new Paragraph($"Generated by SDMS on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC")
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetFontSize(10)
                            //.SetItalic()
                            );
                    }
                }
            }

            return filePath;
        }

        public async Task<string> ExportEmployeeReportToExcelAsync(int startupId)
        {
            var startup = await _context.Startups.FindAsync(startupId);
            if (startup == null)
                throw new ArgumentException("Startup not found");

            var employees = await _context.Employees
                .Include(e => e.User)
                .Where(e => e.StartupId == startupId && e.User.IsActive)
                .OrderBy(e => e.User.Username)
                .ToListAsync();

            // Create directory if it doesn't exist
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Create file path
            var fileName = $"Employee_Report_{startup.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(directory, fileName);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                // Create the Summary worksheet
                var summaryWorksheet = package.Workbook.Worksheets.Add("Summary");
                
                // Add title
                summaryWorksheet.Cells[1, 1].Value = $"Employee Report - {startup.Name}";
                summaryWorksheet.Cells[1, 1, 1, 5].Merge = true;
                summaryWorksheet.Cells[1, 1].Style.Font.Size = 20;
                summaryWorksheet.Cells[1, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                
                // Add report date
                summaryWorksheet.Cells[2, 1].Value = $"Generated on: {DateTime.UtcNow:yyyy-MM-dd}";
                summaryWorksheet.Cells[2, 1, 2, 5].Merge = true;
                summaryWorksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                
                // Add summary
                summaryWorksheet.Cells[4, 1].Value = "Employee Summary";
                summaryWorksheet.Cells[4, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[4, 1].Style.Font.Size = 16;
                
                summaryWorksheet.Cells[5, 1].Value = "Total Employees";
                summaryWorksheet.Cells[5, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[5, 2].Value = employees.Count;
                
                var avgPerformance = employees.Any() ? employees.Average(e => e.PerformanceScore) : 0;
                summaryWorksheet.Cells[6, 1].Value = "Average Performance Score";
                summaryWorksheet.Cells[6, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[6, 2].Value = avgPerformance;
                summaryWorksheet.Cells[6, 2].Style.Numberformat.Format = "0.00";
                
                // Add role distribution
                summaryWorksheet.Cells[8, 1].Value = "Role Distribution";
                summaryWorksheet.Cells[8, 1].Style.Font.Bold = true;
                
                var roleDistribution = employees
                    .GroupBy(e => e.EmployeeRole)
                    .Select(g => new { Role = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();
                
                for (int i = 0; i < roleDistribution.Count; i++)
                {
                    summaryWorksheet.Cells[9 + i, 1].Value = roleDistribution[i].Role;
                    summaryWorksheet.Cells[9 + i, 2].Value = roleDistribution[i].Count;
                }
                
                // Create the Employees worksheet
                var employeesWorksheet = package.Workbook.Worksheets.Add("Employees");
                
                // Add headers
                employeesWorksheet.Cells[1, 1].Value = "Name";
                employeesWorksheet.Cells[1, 2].Value = "Role";
                employeesWorksheet.Cells[1, 3].Value = "Email";
                employeesWorksheet.Cells[1, 4].Value = "Phone";
                employeesWorksheet.Cells[1, 5].Value = "Hire Date";
                employeesWorksheet.Cells[1, 6].Value = "Performance Score";
                
                // Style headers
                for (int i = 1; i <= 6; i++)
                {
                    employeesWorksheet.Cells[1, i].Style.Font.Bold = true;
                }
                
                // Add data
                if (employees.Any())
                {
                    for (int i = 0; i < employees.Count; i++)
                    {
                        var employee = employees[i];
                        int row = i + 2;
                        
                        employeesWorksheet.Cells[row, 1].Value = employee.User.Username;
                        employeesWorksheet.Cells[row, 2].Value = employee.EmployeeRole;
                        employeesWorksheet.Cells[row, 3].Value = employee.User.Email;
                        employeesWorksheet.Cells[row, 4].Value = employee.User.PhoneNumber;
                        employeesWorksheet.Cells[row, 5].Value = employee.HireDate;
                        employeesWorksheet.Cells[row, 5].Style.Numberformat.Format = "yyyy-mm-dd";
                        employeesWorksheet.Cells[row, 6].Value = employee.PerformanceScore;
                        employeesWorksheet.Cells[row, 6].Style.Numberformat.Format = "0.00";
                    }
                }
                else
                {
                    employeesWorksheet.Cells[2, 1].Value = "No employees available.";
                    employeesWorksheet.Cells[2, 1].Style.Font.Italic = true;
                }
                
                // Auto-fit columns
                summaryWorksheet.Cells.AutoFitColumns();
                employeesWorksheet.Cells.AutoFitColumns();
                
                // Save the Excel file
                package.SaveAs(new FileInfo(filePath));
            }

            return filePath;
        }

        public void AddGenericReportContent(Document document, string title, string subtitle, Dictionary<string, string> properties)
        {
            // Add title
            document.Add(new Paragraph(title)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20)
                .SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD)));

            // Add subtitle
            document.Add(new Paragraph(subtitle)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12));

            document.Add(new Paragraph("\n"));

            // Add properties
            if (properties != null && properties.Count > 0)
            {
                var table = new Table(2).UseAllAvailableWidth();
                
                foreach (var property in properties)
                {
                    table.AddCell(new Cell().Add(new Paragraph(property.Key).SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))));
                    table.AddCell(new Cell().Add(new Paragraph(property.Value)));
                }
                
                document.Add(table);
            }
            else
            {
                document.Add(new Paragraph("No data available.")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12)
                 //   .SetItalic()
                    );
            }
        }
    }
}
