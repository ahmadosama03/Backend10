using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SDMS.Core.Services;
using SDMS.Core.DTOs;
// Add the correct using directive if AuditService is in a different namespace
// using YourNamespace.Services;

namespace SDMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly AuditService _auditService;

        public AuditLogsController(AuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResultDto<AuditLogDto>>> GetAuditLogs([FromQuery] PaginationParametersDto parameters)
        {
            var logs = await _auditService.GetAuditLogsAsync(parameters.PageNumber, parameters.PageSize);
            return Ok(logs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuditLogDto>> GetAuditLog(int id)
        {
            var log = await _auditService.GetAuditLogAsync(id);
            if (log == null)
                return NotFound();

            return Ok(log);
        }

        [HttpGet("entity/{entityName}/{entityId}")]
        public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetEntityAuditLogs(string entityName, int entityId)
        {
            var logs = await _auditService.GetEntityAuditLogsAsync(entityName, entityId);
            return Ok(logs);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetUserAuditLogs(int userId)
        {
            var logs = await _auditService.GetUserAuditLogsAsync(userId);
            return Ok(logs);
        }

        [HttpGet("action/{action}")]
        public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetActionAuditLogs(string action)
        {
            var logs = await _auditService.GetActionAuditLogsAsync(action);
            return Ok(logs);
        }

        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetDateRangeAuditLogs(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            var logs = await _auditService.GetDateRangeAuditLogsAsync(startDate, endDate);
            return Ok(logs);
        }
    }
}
