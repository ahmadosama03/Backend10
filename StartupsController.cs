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
    public class StartupsController : ControllerBase
    {
        private readonly StartupService _startupService;

        public StartupsController(StartupService startupService)
        {
            _startupService = startupService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<StartupDto>>> GetStartups()
        {
            var startups = await _startupService.GetStartupsAsync();
            return Ok(startups);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,StartupFounder,Employee")]
        public async Task<ActionResult<StartupDto>> GetStartup(int id)
        {
            var startup = await _startupService.GetStartupAsync(id);
            if (startup == null)
                return NotFound();

            return Ok(startup);
        }

        [HttpGet("founder/{founderId}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<IEnumerable<StartupDto>>> GetStartupsByFounder(int founderId)
        {
            var startups = await _startupService.GetStartupsByFounderAsync(founderId);
            return Ok(startups);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<StartupDto>> CreateStartup(StartupCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var startup = await _startupService.CreateStartupAsync(dto);
            return CreatedAtAction(nameof(GetStartup), new { id = startup.Id }, startup);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<IActionResult> UpdateStartup(int id, StartupUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _startupService.UpdateStartupAsync(id, dto);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStartup(int id)
        {
            var result = await _startupService.DeleteStartupAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id}/dashboard")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<object>> GetStartupDashboard(int id)
        {
            var dashboard = await _startupService.GetStartupDashboardAsync(id);
            if (dashboard == null)
                return NotFound();

            return Ok(dashboard);
        }
    }
}
