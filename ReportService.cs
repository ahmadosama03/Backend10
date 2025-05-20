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

namespace SDMS.Core.Services
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ExportService _exportService;

        public ReportService(ApplicationDbContext context, ExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        public async Task<IEnumerable<ReportDto>> GetReportsAsync(int startupId)
        {
            var reports = await _context.Reports
                .Include(r => r.GeneratedBy)
                .Where(r => r.StartupId == startupId && !r.IsArchived)
                .OrderByDescending(r => r.GeneratedDate)
                .ToListAsync();

            return reports.Select(r => new ReportDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                GeneratedDate = r.GeneratedDate,
                GeneratedBy = r.GeneratedBy.Username,
                ReportType = r.ReportType,
                Format = r.Format,
                StoragePath = r.StoragePath,
                StartupId = r.StartupId
            });
        }

        public async Task<ReportDto> GetReportAsync(int id)
        {
            var report = await _context.Reports
                .Include(r => r.GeneratedBy)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                return null;

            return new ReportDto
            {
                Id = report.Id,
                Title = report.Title,
                Description = report.Description,
                GeneratedDate = report.GeneratedDate,
                GeneratedBy = report.GeneratedBy.Username,
                ReportType = report.ReportType,
                Format = report.Format,
                StoragePath = report.StoragePath,
                StartupId = report.StartupId
            };
        }

        public async Task<ReportDto> GenerateFinancialReportAsync(ReportGenerateDto dto)
        {
            var startup = await _context.Startups.FindAsync(dto.StartupId);
            if (startup == null)
                throw new ArgumentException("Startup not found");

            string filePath;
            if (dto.Format.ToLower() == "pdf")
            {
                filePath = await _exportService.ExportFinancialReportToPdfAsync(
                    dto.StartupId, 
                    dto.StartDate, 
                    dto.EndDate);
            }
            else if (dto.Format.ToLower() == "excel")
            {
                filePath = await _exportService.ExportFinancialReportToExcelAsync(
                    dto.StartupId, 
                    dto.StartDate, 
                    dto.EndDate);
            }
            else
            {
                throw new ArgumentException("Unsupported format. Use 'PDF' or 'Excel'.");
            }

            var report = new Report
            {
                Title = $"Financial Report - {startup.Name}",
                Description = $"Financial report for period {dto.StartDate:yyyy-MM-dd} to {dto.EndDate:yyyy-MM-dd}",
                GeneratedDate = DateTime.UtcNow,
                GeneratedById = dto.GeneratedById,
                ReportType = "Financial",
                Format = dto.Format,
                StoragePath = filePath,
                StartupId = dto.StartupId
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return new ReportDto
            {
                Id = report.Id,
                Title = report.Title,
                Description = report.Description,
                GeneratedDate = report.GeneratedDate,
                GeneratedBy = (await _context.Users.FindAsync(report.GeneratedById))?.Username,
                ReportType = report.ReportType,
                Format = report.Format,
                StoragePath = report.StoragePath,
                StartupId = report.StartupId
            };
        }

        public async Task<ReportDto> GenerateEmployeeReportAsync(ReportGenerateDto dto)
        {
            var startup = await _context.Startups.FindAsync(dto.StartupId);
            if (startup == null)
                throw new ArgumentException("Startup not found");

            string filePath;
            if (dto.Format.ToLower() == "pdf")
            {
                filePath = await _exportService.ExportEmployeeReportToPdfAsync(dto.StartupId);
            }
            else if (dto.Format.ToLower() == "excel")
            {
                filePath = await _exportService.ExportEmployeeReportToExcelAsync(dto.StartupId);
            }
            else
            {
                throw new ArgumentException("Unsupported format. Use 'PDF' or 'Excel'.");
            }

            var report = new Report
            {
                Title = $"Employee Report - {startup.Name}",
                Description = $"Employee status report as of {DateTime.UtcNow:yyyy-MM-dd}",
                GeneratedDate = DateTime.UtcNow,
                GeneratedById = dto.GeneratedById,
                ReportType = "Employee",
                Format = dto.Format,
                StoragePath = filePath,
                StartupId = dto.StartupId
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return new ReportDto
            {
                Id = report.Id,
                Title = report.Title,
                Description = report.Description,
                GeneratedDate = report.GeneratedDate,
                GeneratedBy = (await _context.Users.FindAsync(report.GeneratedById))?.Username,
                ReportType = report.ReportType,
                Format = report.Format,
                StoragePath = report.StoragePath,
                StartupId = report.StartupId
            };
        }

        public async Task<bool> ArchiveReportAsync(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                return false;

            report.IsArchived = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> DownloadReportAsync(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                throw new ArgumentException("Report not found");

            if (!File.Exists(report.StoragePath))
                throw new FileNotFoundException("Report file not found");

            return report.StoragePath;
        }

        public void AddGenericReportContent(Document document, string title, string subtitle, Dictionary<string, string> properties)
        {
            // Add title
            document.Add(new Paragraph(title)
                .SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20));

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
                    table.AddCell(new Cell().Add(new Paragraph(property.Key)));//setbold was deleted
                    table.AddCell(new Cell().Add(new Paragraph(property.Value)));
                }
                
                document.Add(table);
            }
            else
            {
                document.Add(new Paragraph("No data available.")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12)
                  //  .SetItalic()
                    );
            }
        }
    }
}
