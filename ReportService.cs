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
using iText.Kernel.Font; // Added for font factory
using iText.IO.Font.Constants; // Added for standard fonts

namespace SDMS.Core.Services
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ExportService _exportService;
        private readonly PdfFont _boldFont;
        private readonly PdfFont _italicFont;

        public ReportService(ApplicationDbContext context, ExportService exportService)
        {
            _context = context;
            _exportService = exportService;
            // Pre-create fonts for efficiency
            _boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            _italicFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);
        }

        public async Task<IEnumerable<ReportDto>> GetReportsAsync(int startupId)
        {
            var reports = await _context.Reports
                .Include(r => r.GeneratedBy) // Include the User navigation property
                .Where(r => r.StartupId == startupId) 
                .OrderByDescending(r => r.GeneratedDate)
                .ToListAsync();

            // Use AutoMapper if configured, otherwise manual mapping
            return reports.Select(r => new ReportDto
            {
                Id = r.Id,
                GeneratedDate = r.GeneratedDate,
                GeneratedByName = r.GeneratedBy?.Name ?? "System", // Use User.Name
                ReportType = r.ReportType,
                // Format = r.Format, // Format property removed from Report entity
                FilePath = r.FilePath, // FilePath is on Report entity
                StartupId = r.StartupId,
                GeneratedById = r.GeneratedById,
                Parameters = r.Parameters
            });
        }

        public async Task<ReportDto> GetReportAsync(int id)
        {
            var report = await _context.Reports
                .Include(r => r.GeneratedBy) // Include the User navigation property
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                return null;

            // Use AutoMapper if configured, otherwise manual mapping
            return new ReportDto
            {
                Id = report.Id,
                GeneratedDate = report.GeneratedDate,
                GeneratedByName = report.GeneratedBy?.Name ?? "System", // Use User.Name
                ReportType = report.ReportType,
                // Format = report.Format, // Format property removed from Report entity
                FilePath = report.FilePath, // FilePath is on Report entity
                StartupId = report.StartupId,
                GeneratedById = report.GeneratedById,
                Parameters = report.Parameters
            };
        }

        public async Task<ReportDto> GenerateFinancialReportAsync(ReportGenerateDto dto)
        {
            if (!dto.StartupId.HasValue)
                throw new ArgumentException("Startup ID is required for financial reports.");
                
            var startup = await _context.Startups.FindAsync(dto.StartupId.Value);
            if (startup == null)
                throw new ArgumentException("Startup not found");

            string filePath;
            if (dto.Format.ToLower() == "pdf")
            {
                filePath = await _exportService.ExportFinancialReportToPdfAsync(
                    dto.StartupId.Value, 
                    dto.StartDate, 
                    dto.EndDate);
            }
            else if (dto.Format.ToLower() == "excel")
            {
                filePath = await _exportService.ExportFinancialReportToExcelAsync(
                    dto.StartupId.Value, 
                    dto.StartDate, 
                    dto.EndDate);
            }
            else
            {
                throw new ArgumentException("Unsupported format. Use 'pdf' or 'excel'.");
            }

            var report = new Report
            {
                GeneratedDate = DateTime.UtcNow,
                GeneratedById = dto.GeneratedById,
                ReportType = "Financial",
                // Format = dto.Format, // Format property removed from Report entity
                FilePath = filePath,
                StartupId = dto.StartupId,
                Parameters = System.Text.Json.JsonSerializer.Serialize(new { dto.StartDate, dto.EndDate }) // Store parameters
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var generatedByUser = await _context.Users.FindAsync(report.GeneratedById);
            // Use AutoMapper or manual mapping
            return new ReportDto
            {
                Id = report.Id,
                GeneratedDate = report.GeneratedDate,
                GeneratedByName = generatedByUser?.Name ?? "System",
                ReportType = report.ReportType,
                // Format = report.Format, // Format property removed from Report entity
                FilePath = report.FilePath,
                StartupId = report.StartupId,
                GeneratedById = report.GeneratedById,
                Parameters = report.Parameters
            };
        }

        public async Task<ReportDto> GenerateEmployeeReportAsync(ReportGenerateDto dto)
        {
             if (!dto.StartupId.HasValue)
                throw new ArgumentException("Startup ID is required for employee reports.");

            var startup = await _context.Startups.FindAsync(dto.StartupId.Value);
            if (startup == null)
                throw new ArgumentException("Startup not found");

            string filePath;
            if (dto.Format.ToLower() == "pdf")
            {
                filePath = await _exportService.ExportEmployeeReportToPdfAsync(dto.StartupId.Value);
            }
            else if (dto.Format.ToLower() == "excel")
            {
                filePath = await _exportService.ExportEmployeeReportToExcelAsync(dto.StartupId.Value);
            }
            else
            {
                throw new ArgumentException("Unsupported format. Use 'pdf' or 'excel'.");
            }

            var report = new Report
            {
                GeneratedDate = DateTime.UtcNow,
                GeneratedById = dto.GeneratedById,
                ReportType = "Employee",
                // Format = dto.Format, // Format property removed from Report entity
                FilePath = filePath,
                StartupId = dto.StartupId
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var generatedByUser = await _context.Users.FindAsync(report.GeneratedById);
            // Use AutoMapper or manual mapping
            return new ReportDto
            {
                Id = report.Id,
                GeneratedDate = report.GeneratedDate,
                GeneratedByName = generatedByUser?.Name ?? "System",
                ReportType = report.ReportType,
                // Format = report.Format, // Format property removed from Report entity
                FilePath = report.FilePath,
                StartupId = report.StartupId,
                GeneratedById = report.GeneratedById,
                Parameters = report.Parameters
            };
        }

        public async Task<bool> ArchiveReportAsync(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                return false;

            _context.Reports.Remove(report); // Example: Delete instead of archiving if no IsArchived flag
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> DownloadReportAsync(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                throw new ArgumentException("Report not found");

            if (!File.Exists(report.FilePath))
                throw new FileNotFoundException("Report file not found", report.FilePath);

            return report.FilePath;
        }

        // This method seems generic and might belong in ExportService or a utility class
        // It also uses iText7 directly, which might be inconsistent if other methods use different libraries
        // Corrected SetBold/SetItalic usage
        public void AddGenericReportContent(Document document, string title, string subtitle, Dictionary<string, string> properties)
        {
            // Add title
            document.Add(new Paragraph(title)
                .SetFont(_boldFont) // Use pre-defined bold font
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
                    table.AddCell(new Cell().Add(new Paragraph(property.Key).SetFont(_boldFont))); // Use pre-defined bold font
                    table.AddCell(new Cell().Add(new Paragraph(property.Value)));
                }
                
                document.Add(table);
            }
            else
            {
                document.Add(new Paragraph("No data available.")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12)
                    .SetFont(_italicFont) // Use pre-defined italic font
                    );
            }
        }
    }
}

