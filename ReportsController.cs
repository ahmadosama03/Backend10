using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SDMS.Core.Services;
using SDMS.Core.DTOs;

namespace SDMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportsController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("startup/{startupId}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<IEnumerable<ReportDto>>> GetReports(int startupId)
        {
            var reports = await _reportService.GetReportsAsync(startupId);
            return Ok(reports);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<ReportDto>> GetReport(int id)
        {
            var report = await _reportService.GetReportAsync(id);
            if (report == null)
                return NotFound();

            return Ok(report);
        }

        [HttpPost("financial")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<ReportDto>> GenerateFinancialReport(ReportGenerateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var report = await _reportService.GenerateFinancialReportAsync(dto);
                return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while generating the report");
            }
        }

        [HttpPost("employee")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<ReportDto>> GenerateEmployeeReport(ReportGenerateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var report = await _reportService.GenerateEmployeeReportAsync(dto);
                return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while generating the report");
            }
        }

        [HttpPost("{id}/archive")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<IActionResult> ArchiveReport(int id)
        {
            var result = await _reportService.ArchiveReportAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id}/download")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<IActionResult> DownloadReport(int id)
        {
            try
            {
                var filePath = await _reportService.DownloadReportAsync(id);
                var fileName = System.IO.Path.GetFileName(filePath);
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var fileExtension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
                
                string contentType;
                switch (fileExtension)
                {
                    case ".pdf":
                        contentType = "application/pdf";
                        break;
                    case ".xlsx":
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;
                    default:
                        contentType = "application/octet-stream";
                        break;
                }
                
                return File(fileBytes, contentType, fileName);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (System.IO.FileNotFoundException)
            {
                return NotFound("Report file not found");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while downloading the report");
            }
        }
    }
}
