using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SDMS.Core.DTOs;
using SDMS.Domain.Entities;
using SDMS.Infrastructure.Data;
using AutoMapper;

namespace SDMS.Core.Services
{
    /// <summary>
    /// Service for managing startups in the SDMS system
    /// </summary>
    public class StartupService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly AuditService _auditService;
        private readonly FinancialService _financialService;

        public StartupService(
            ApplicationDbContext context,
            IMapper mapper,
            AuditService auditService,
            FinancialService financialService)
        {
            _context = context;
            _mapper = mapper;
            _auditService = auditService;
            _financialService = financialService;
        }

        /// <summary>
        /// Gets all startups
        /// </summary>
        /// <returns>List of startup DTOs</returns>
        public async Task<IEnumerable<StartupDto>> GetStartupsAsync()
        {
            var startups = await _context.Startups
                .Include(s => s.Founder)
                    .ThenInclude(f => f.User)
                .ToListAsync();

            return _mapper.Map<IEnumerable<StartupDto>>(startups);
        }

        /// <summary>
        /// Gets a specific startup by ID
        /// </summary>
        /// <param name="id">Startup ID</param>
        /// <returns>Startup DTO or null if not found</returns>
        public async Task<StartupDto> GetStartupAsync(int id)
        {
            var startup = await _context.Startups
                .Include(s => s.Founder)
                    .ThenInclude(f => f.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            return _mapper.Map<StartupDto>(startup);
        }

        /// <summary>
        /// Gets startups by founder ID
        /// </summary>
        /// <param name="founderId">Founder ID</param>
        /// <returns>List of startup DTOs</returns>
        public async Task<IEnumerable<StartupDto>> GetStartupsByFounderAsync(int founderId)
        {
            var startups = await _context.Startups
                .Include(s => s.Founder)
                    .ThenInclude(f => f.User)
                .Where(s => s.FounderId == founderId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<StartupDto>>(startups);
        }

        /// <summary>
        /// Creates a new startup
        /// </summary>
        /// <param name="createDto">Startup creation data</param>
        /// <returns>Created startup DTO</returns>
        public async Task<StartupDto> CreateStartupAsync(StartupCreateDto createDto)
        {
            // Check if founder exists
            var founder = await _context.StartupFounders.FindAsync(createDto.FounderId);
            if (founder == null)
                throw new ArgumentException("Founder not found");

            var startup = _mapper.Map<Startup>(createDto);
            
            _context.Startups.Add(startup);
            await _context.SaveChangesAsync();

            // Log startup creation
            await _auditService.LogEntityChangeAsync("Create", startup.Id, null, startup);

            return _mapper.Map<StartupDto>(startup);
        }

        /// <summary>
        /// Updates a startup
        /// </summary>
        /// <param name="id">Startup ID</param>
        /// <param name="updateDto">Startup update data</param>
        /// <returns>True if update is successful, false otherwise</returns>
        public async Task<bool> UpdateStartupAsync(int id, StartupUpdateDto updateDto)
        {
            var startup = await _context.Startups.FindAsync(id);
            if (startup == null)
                return false;

            var oldStartup = new
            {
                startup.Name,
                startup.Industry,
                startup.Description,
                startup.LogoUrl
            };

            startup.Name = updateDto.Name;
            startup.Industry = updateDto.Industry;
            startup.Description = updateDto.Description;
            startup.LogoUrl = updateDto.LogoUrl;

            _context.Startups.Update(startup);
            await _context.SaveChangesAsync();

            // Log startup update
            await _auditService.LogEntityChangeAsync<object>("Update", id, oldStartup, new
            {
                updateDto.Name,
                updateDto.Industry,
                updateDto.Description,
                updateDto.LogoUrl
            });

            return true;
        }

        /// <summary>
        /// Deletes a startup
        /// </summary>
        /// <param name="id">Startup ID</param>
        /// <returns>True if deletion is successful, false otherwise</returns>
        public async Task<bool> DeleteStartupAsync(int id)
        {
            var startup = await _context.Startups.FindAsync(id);
            if (startup == null)
                return false;

            // Check for related entities and handle them
            var employees = await _context.Employees.Where(e => e.StartupId == id).ToListAsync();
            if (employees.Any())
            {
                // Either delete employees or handle differently based on requirements
                _context.Employees.RemoveRange(employees);
            }

            // Store startup data for logging
            var startupData = new
            {
                startup.Id,
                startup.Name,
                startup.Industry,
                startup.FounderId,
                startup.FoundingDate
            };

            _context.Startups.Remove(startup);
            await _context.SaveChangesAsync();

            // Log startup deletion
            await _auditService.LogEntityChangeAsync("Delete", id, startupData, null);

            return true;
        }

        /// <summary>
        /// Gets a startup dashboard with key metrics
        /// </summary>
        /// <param name="id">Startup ID</param>
        /// <returns>Dashboard data or null if startup not found</returns>
        public async Task<object> GetStartupDashboardAsync(int id)
        {
            var startup = await _context.Startups
                .Include(s => s.Founder)
                    .ThenInclude(f => f.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (startup == null)
                return null;

            // Get employee count
            var employeeCount = await _context.Employees
                .CountAsync(e => e.StartupId == id);

            // Get financial metrics
            var financialSummary = await _financialService.GetFinancialSummaryAsync(id, null, null);

            // Get subscription status
            var subscription = await _context.Subscriptions
                .Where(s => s.StartupId == id && s.IsActive)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            // Get recent financial metrics
            var recentMetrics = await _context.FinancialMetrics
                .Where(m => m.StartupId == id)
                .OrderByDescending(m => m.Date)
                .Take(5)
                .ToListAsync();

            // Get recent notifications
            var recentNotifications = await _context.Notifications
                .Where(n => n.User.Employee != null && n.User.Employee.StartupId == id)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            return new
            {
                Startup = _mapper.Map<StartupDto>(startup),
                EmployeeCount = employeeCount,
                FinancialSummary = financialSummary,
                Subscription = subscription != null ? new
                {
                    subscription.PlanType,
                    subscription.EndDate,
                    subscription.IsActive,
                    DaysRemaining = (subscription.EndDate - DateTime.UtcNow).Days
                } : null,
                RecentMetrics = _mapper.Map<IEnumerable<FinancialMetricDto>>(recentMetrics),
                RecentNotifications = _mapper.Map<IEnumerable<NotificationDto>>(recentNotifications)
            };
        }

        /// <summary>
        /// Updates a startup's subscription status
        /// </summary>
        /// <param name="id">Startup ID</param>
        /// <param name="status">New subscription status</param>
        /// <returns>True if update is successful, false otherwise</returns>
        public async Task<bool> UpdateSubscriptionStatusAsync(int id, string status)
        {
            var startup = await _context.Startups.FindAsync(id);
            if (startup == null)
                return false;

            var oldStatus = startup.SubscriptionStatus;
            startup.SubscriptionStatus = status;

            _context.Startups.Update(startup);
            await _context.SaveChangesAsync();

            // Log subscription status update
            await _auditService.LogEntityChangeAsync("Update", id, 
                new { SubscriptionStatus = oldStatus }, 
                new { SubscriptionStatus = status });

            return true;
        }

        /// <summary>
        /// Gets startups by industry
        /// </summary>
        /// <param name="industry">Industry name</param>
        /// <returns>List of startup DTOs</returns>
        public async Task<IEnumerable<StartupDto>> GetStartupsByIndustryAsync(string industry)
        {
            var startups = await _context.Startups
                .Include(s => s.Founder)
                    .ThenInclude(f => f.User)
                .Where(s => s.Industry == industry)
                .ToListAsync();

            return _mapper.Map<IEnumerable<StartupDto>>(startups);
        }

        /// <summary>
        /// Gets startups founded within a date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of startup DTOs</returns>
        public async Task<IEnumerable<StartupDto>> GetStartupsByFoundingDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var startups = await _context.Startups
                .Include(s => s.Founder)
                    .ThenInclude(f => f.User)
                .Where(s => s.FoundingDate >= startDate && s.FoundingDate <= endDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<StartupDto>>(startups);
        }

        /// <summary>
        /// Gets startups by subscription status
        /// </summary>
        /// <param name="status">Subscription status</param>
        /// <returns>List of startup DTOs</returns>
        public async Task<IEnumerable<StartupDto>> GetStartupsBySubscriptionStatusAsync(string status)
        {
            var startups = await _context.Startups
                .Include(s => s.Founder)
                    .ThenInclude(f => f.User)
                .Where(s => s.SubscriptionStatus == status)
                .ToListAsync();

            return _mapper.Map<IEnumerable<StartupDto>>(startups);
        }

        /// <summary>
        /// Searches startups by name or industry
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of startup DTOs</returns>
        public async Task<IEnumerable<StartupDto>> SearchStartupsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetStartupsAsync();

            var normalizedSearchTerm = searchTerm.ToLower();

            var startups = await _context.Startups
                .Include(s => s.Founder)
                    .ThenInclude(f => f.User)
                .Where(s => s.Name.ToLower().Contains(normalizedSearchTerm) || 
                           s.Industry.ToLower().Contains(normalizedSearchTerm))
                .ToListAsync();

            return _mapper.Map<IEnumerable<StartupDto>>(startups);
        }

        /// <summary>
        /// Gets industry statistics
        /// </summary>
        /// <returns>Industry statistics</returns>
        public async Task<object> GetIndustryStatisticsAsync()
        {
            var industries = await _context.Startups
                .Where(s => !string.IsNullOrEmpty(s.Industry))
                .GroupBy(s => s.Industry)
                .Select(g => new
                {
                    Industry = g.Key,
                    Count = g.Count(),
                    AverageAge = g.Average(s => (DateTime.UtcNow - s.FoundingDate).Days) / 365.0
                })
                .ToListAsync();

            return new
            {
                Industries = industries,
                TotalStartups = await _context.Startups.CountAsync(),
                NewestIndustry = industries.OrderByDescending(i => i.AverageAge).FirstOrDefault()?.Industry,
                MostPopularIndustry = industries.OrderByDescending(i => i.Count).FirstOrDefault()?.Industry
            };
        }

        /// <summary>
        /// Gets growth metrics for a startup
        /// </summary>
        /// <param name="id">Startup ID</param>
        /// <returns>Growth metrics or null if startup not found</returns>
        public async Task<object> GetGrowthMetricsAsync(int id)
        {
            var startup = await _context.Startups.FindAsync(id);
            if (startup == null)
                return null;

            // Get financial metrics for the past year
            var oneYearAgo = DateTime.UtcNow.AddYears(-1);
            var financialMetrics = await _context.FinancialMetrics
                .Where(m => m.StartupId == id && m.Date >= oneYearAgo)
                .OrderBy(m => m.Date)
                .ToListAsync();

            // Calculate revenue growth
            var revenueGrowth = CalculateGrowthRate(financialMetrics.Select(m => m.Revenue).ToList());

            // Calculate employee growth
            var employeeHistory = await _context.Employees
                .Where(e => e.StartupId == id)
                .GroupBy(e => e.HireDate.Year * 100 + e.HireDate.Month)
                .Select(g => new
                {
                    YearMonth = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.YearMonth)
                .ToListAsync();

            var employeeGrowth = employeeHistory.Count > 1 ? 
                (double)(employeeHistory.Last().Count - employeeHistory.First().Count) / employeeHistory.First().Count * 100 : 
                0;

            return new
            {
                RevenueGrowth = revenueGrowth,
                EmployeeGrowth = employeeGrowth,
                MonthlyData = financialMetrics.GroupBy(m => new { m.Date.Year, m.Date.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Revenue = g.Sum(m => m.Revenue),
                        Expenses = g.Sum(m => m.Expenses),
                        Profit = g.Sum(m => m.Revenue - m.Expenses)
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// Calculates growth rate from a list of values
        /// </summary>
        /// <param name="values">List of values</param>
        /// <returns>Growth rate percentage</returns>
        private double CalculateGrowthRate(List<float> values)
        {
            if (values.Count < 2 || values[0] == 0)
                return 0;

            var firstValue = values.First();
            var lastValue = values.Last();

            return (double)(lastValue - firstValue) / firstValue * 100;
        }
    }
}
