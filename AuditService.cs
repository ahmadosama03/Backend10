using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SDMS.Domain.Entities;
using SDMS.Infrastructure.Data;
using SDMS.Core.DTOs;
using AutoMapper;
using System.Text.Json;

namespace SDMS.Core.Services
{
    /// <summary>
    /// Service for handling audit logging functionality in the SDMS system
    /// </summary>
    public class AuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(
            ApplicationDbContext context,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Logs an audit entry for entity changes
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="action">Action performed (Create, Update, Delete)</param>
        /// <param name="entityName">Name of the entity</param>
        /// <param name="entityId">ID of the entity</param>
        /// <param name="oldValues">Previous state of the entity (null for Create)</param>
        /// <param name="newValues">New state of the entity (null for Delete)</param>
        /// <returns>The created audit log entry</returns>
        public async Task<AuditLog> LogEntityChangeAsync<T>(
            string action,
            int? entityId,
            T oldValues = default,
            T newValues = default) where T : class
        {
            var entityName = typeof(T).Name;
            var userId = GetCurrentUserId();
            var ipAddress = GetCurrentIpAddress();

            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                OldValues = oldValues != null ? JsonConvert.SerializeObject(oldValues) : null,
                NewValues = newValues != null ? JsonConvert.SerializeObject(newValues) : null,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return auditLog;
        }

        /// <summary>
        /// Logs a user action (login, logout, etc.)
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="action">Action performed</param>
        /// <param name="details">Additional details</param>
        /// <returns>The created audit log entry</returns>
        public async Task<AuditLog> LogUserActionAsync(int userId, string action, string details = null)
        {
            var ipAddress = GetCurrentIpAddress();

            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = "User",
                EntityId = userId,
                NewValues = details,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return auditLog;
        }

        /// <summary>
        /// Logs a system event
        /// </summary>
        /// <param name="action">Action or event type</param>
        /// <param name="details">Event details</param>
        /// <returns>The created audit log entry</returns>
        public async Task<AuditLog> LogSystemEventAsync(string action, string details)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                EntityName = "System",
                NewValues = details,
                Timestamp = DateTime.UtcNow,
                IpAddress = GetCurrentIpAddress()
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return auditLog;
        }

        /// <summary>
        /// Gets all audit logs with pagination
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated list of audit logs</returns>
        public async Task<PaginatedResultDto<AuditLogDto>> GetAuditLogsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var auditLogs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var auditLogDtos = _mapper.Map<List<AuditLogDto>>(auditLogs);

            return new PaginatedResultDto<AuditLogDto>
            {
                Items = auditLogDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            };
        }

        /// <summary>
        /// Gets a specific audit log by ID
        /// </summary>
        /// <param name="id">Audit log ID</param>
        /// <returns>Audit log DTO</returns>
        public async Task<AuditLogDto> GetAuditLogAsync(int id)
        {
            var auditLog = await _context.AuditLogs
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            return _mapper.Map<AuditLogDto>(auditLog);
        }

        /// <summary>
        /// Gets audit logs for a specific entity
        /// </summary>
        /// <param name="entityName">Entity name</param>
        /// <param name="entityId">Entity ID</param>
        /// <returns>List of audit log DTOs</returns>
        public async Task<IEnumerable<AuditLogDto>> GetEntityAuditLogsAsync(string entityName, int entityId)
        {
            var auditLogs = await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AuditLogDto>>(auditLogs);
        }

        /// <summary>
        /// Gets audit logs for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of audit log DTOs</returns>
        public async Task<IEnumerable<AuditLogDto>> GetUserAuditLogsAsync(int userId)
        {
            var auditLogs = await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AuditLogDto>>(auditLogs);
        }

        /// <summary>
        /// Gets audit logs for a specific action
        /// </summary>
        /// <param name="action">Action name</param>
        /// <returns>List of audit log DTOs</returns>
        public async Task<IEnumerable<AuditLogDto>> GetActionAuditLogsAsync(string action)
        {
            var auditLogs = await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.Action == action)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AuditLogDto>>(auditLogs);
        }

        /// <summary>
        /// Gets audit logs within a date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of audit log DTOs</returns>
        public async Task<IEnumerable<AuditLogDto>> GetDateRangeAuditLogsAsync(DateTime startDate, DateTime endDate)
        {
            var auditLogs = await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AuditLogDto>>(auditLogs);
        }

        /// <summary>
        /// Gets the current user ID from the HTTP context
        /// </summary>
        /// <returns>User ID or null</returns>
        private int? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// Gets the current IP address from the HTTP context
        /// </summary>
        /// <returns>IP address</returns>
        private string GetCurrentIpAddress()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return "Unknown";
            }

            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        /// <summary>
        /// Compares two objects and returns a dictionary of changed properties
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="oldObject">Old object state</param>
        /// <param name="newObject">New object state</param>
        /// <returns>Dictionary of changed properties</returns>
        public Dictionary<string, object> GetChangedProperties<T>(T oldObject, T newObject) where T : class
        {
            var changes = new Dictionary<string, object>();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var oldValue = property.GetValue(oldObject);
                var newValue = property.GetValue(newObject);

                if (!Equals(oldValue, newValue))
                {
                    changes[property.Name] = new
                    {
                        OldValue = oldValue,
                        NewValue = newValue
                    };
                }
            }

            return changes;
        }

        /// <summary>
        /// Purges audit logs older than the specified retention period
        /// </summary>
        /// <param name="retentionDays">Number of days to retain logs</param>
        /// <returns>Number of purged logs</returns>
        public async Task<int> PurgeOldAuditLogsAsync(int retentionDays)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            
            var logsToDelete = await _context.AuditLogs
                .Where(a => a.Timestamp < cutoffDate)
                .ToListAsync();
            
            _context.AuditLogs.RemoveRange(logsToDelete);
            await _context.SaveChangesAsync();
            
            return logsToDelete.Count;
        }

        /// <summary>
        /// Exports audit logs to JSON format
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>JSON string of audit logs</returns>
        public async Task<string> ExportAuditLogsToJsonAsync(DateTime startDate, DateTime endDate)
        {
            var auditLogs = await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            var auditLogDtos = _mapper.Map<IEnumerable<AuditLogDto>>(auditLogs);
            
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            return System.Text.Json.JsonSerializer.Serialize(auditLogDtos, options);
        }
    }
}
