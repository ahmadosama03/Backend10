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
    public class FinancialMetricsController : ControllerBase
    {
        private readonly FinancialService _financialService;

        public FinancialMetricsController(FinancialService financialService)
        {
            _financialService = financialService;
        }

        [HttpGet("startup/{startupId}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<IEnumerable<FinancialMetricDto>>> GetFinancialMetrics(int startupId)
        {
            var metrics = await _financialService.GetFinancialMetricsAsync(startupId);
            return Ok(metrics);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<FinancialMetricDto>> GetFinancialMetric(int id)
        {
            var metric = await _financialService.GetFinancialMetricAsync(id);
            if (metric == null)
                return NotFound();

            return Ok(metric);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<FinancialMetricDto>> CreateFinancialMetric(FinancialMetricCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var metric = await _financialService.CreateFinancialMetricAsync(dto);
            return CreatedAtAction(nameof(GetFinancialMetric), new { id = metric.Id }, metric);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<IActionResult> UpdateFinancialMetric(int id, FinancialMetricUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _financialService.UpdateFinancialMetricAsync(id, dto);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<IActionResult> DeleteFinancialMetric(int id)
        {
            var result = await _financialService.DeleteFinancialMetricAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("summary/{startupId}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<FinancialSummaryDto>> GetFinancialSummary(
            int startupId, 
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null)
        {
            var summary = await _financialService.GetFinancialSummaryAsync(startupId, startDate, endDate);
            return Ok(summary);
        }

        [HttpGet("growth/{startupId}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<GrowthAnalysisDto>> GetGrowthAnalysis(int startupId)
        {
            var analysis = await _financialService.CalculateGrowthRatesAsync(startupId);
            return Ok(analysis);
        }
    }
}
