using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Add if needed
using SDMS.Core.Services;
using SDMS.Core.DTOs;

namespace SDMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Templates seem public based on frontend mock, add if auth is required
    public class TemplatesController : ControllerBase
    {
        private readonly TemplateService _templateService;

        public TemplatesController(TemplateService templateService)
        {
            _templateService = templateService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TemplateDto>>> GetTemplates()
        {
            var templates = await _templateService.GetTemplatesAsync();
            return Ok(templates);
        }

        [HttpGet("{id}")] // Frontend uses string identifier like 'physical'
        public async Task<ActionResult<TemplateDto>> GetTemplate(string id)
        {
            var template = await _templateService.GetTemplateByIdAsync(id);
            if (template == null)
                return NotFound();

            return Ok(template);
        }

        // Add POST, PUT, DELETE endpoints if template management is required later
    }
}

