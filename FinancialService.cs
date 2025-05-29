using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SDMS.Domain.Entities;
using SDMS.Core.DTOs;
using System.Linq;
using System.Text.Json;
using SDMS.Infrastructure.Data;
using AutoMapper; // Added AutoMapper

namespace SDMS.Core.Services
{
    public class FinancialService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Added AutoMapper

        public FinancialService(ApplicationDbContext context, IMapper mapper) // Added AutoMapper
        {
            _context = context;
            _mapper = mapper; // Added AutoMapper
        }

        // Refactored to use the new entity structure and DTO
        public async Task<IEnumerable<FinancialMetricDto>> GetFinancialMetricsAsync(int startupId)
        {
            var metrics = await _context.FinancialMetrics
                .Where(fm => fm.StartupId == startupId && !fm.IsArchived)
                .OrderByDescending(fm => fm.Date)
                .ToListAsync();

            return _mapper.Map<IEnumerable<FinancialMetricDto>>(metrics); // Use AutoMapper
        }

        // Refactored to use the new entity structure and DTO
        public async Task<FinancialMetricDto> GetFinancialMetricAsync(int id)
        {
            var metric = await _context.FinancialMetrics
                .FirstOrDefaultAsync(fm => fm.Id == id);

            return _mapper.Map<FinancialMetricDto>(metric); // Use AutoMapper
        }

        // Refactored to use the new entity structure and DTO
        public async Task<FinancialMetricDto> CreateFinancialMetricAsync(FinancialMetricCreateDto dto)
        {
            var metric = _mapper.Map<FinancialMetric>(dto); // Use AutoMapper
            metric.Date = dto.Date ?? DateTime.UtcNow; // Ensure date is set

            _context.FinancialMetrics.Add(metric);
            await _context.SaveChangesAsync();

            return _mapper.Map<FinancialMetricDto>(metric); // Use AutoMapper
        }

        // Refactored to use the new entity structure and DTO
        public async Task<bool> UpdateFinancialMetricAsync(int id, FinancialMetricUpdateDto dto)
        {
            var metric = await _context.FinancialMetrics.FindAsync(id);
            if (metric == null || metric.IsArchived)
                return false;

            _mapper.Map(dto, metric); // Use AutoMapper to update fields
            metric.Date = dto.Date ?? metric.Date; // Update date if provided

            _context.FinancialMetrics.Update(metric);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFinancialMetricAsync(int id)
        {
            var metric = await _context.FinancialMetrics.FindAsync(id);
            if (metric == null)
                return false;

            metric.IsArchived = true;
            _context.FinancialMetrics.Update(metric); // Mark as updated
            await _context.SaveChangesAsync();
            return true;
        }

        // Refactored to use MetricType and Value
        public async Task<FinancialSummaryDto> GetFinancialSummaryAsync(int startupId, DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddMonths(-3);
            endDate ??= DateTime.UtcNow;

            var metrics = await _context.FinancialMetrics
                .Where(fm => fm.StartupId == startupId && 
                       !fm.IsArchived && 
                       fm.Date >= startDate && 
                       fm.Date <= endDate)
                .ToListAsync();

            if (!metrics.Any())
            {
                return new FinancialSummaryDto
                {
                    StartupId = startupId,
                    StartDate = startDate.Value,
                    EndDate = endDate.Value,
                    TotalRevenue = 0,
                    TotalExpenses = 0,
                    TotalSales = 0, // Assuming Sales means units sold
                    NetProfit = 0,
                    MetricsCount = 0,
                    MonthlyData = new List<MonthlyFinancialDataDto>()
                };
            }

            var totalRevenue = metrics.Where(m => m.MetricType == "Revenue").Sum(m => m.Value);
            var totalExpenses = metrics.Where(m => m.MetricType == "Expense").Sum(m => m.Value);
            var totalSales = metrics.Where(m => m.MetricType == "SalesUnits").Sum(m => m.Value); // Assuming "SalesUnits"

            var summary = new FinancialSummaryDto
            {
                StartupId = startupId,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalRevenue = totalRevenue,
                TotalExpenses = totalExpenses,
                TotalSales = totalSales,
                NetProfit = totalRevenue - totalExpenses,
                MetricsCount = metrics.Count,
                MonthlyData = metrics
                    .GroupBy(m => new { m.Date.Year, m.Date.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .Select(g => new MonthlyFinancialDataDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Revenue = g.Where(m => m.MetricType == "Revenue").Sum(m => m.Value),
                        Expenses = g.Where(m => m.MetricType == "Expense").Sum(m => m.Value),
                        Sales = g.Where(m => m.MetricType == "SalesUnits").Sum(m => m.Value),
                        Profit = g.Where(m => m.MetricType == "Revenue").Sum(m => m.Value) - g.Where(m => m.MetricType == "Expense").Sum(m => m.Value)
                    })
                    .ToList()
            };

            return summary;
        }

        // Refactored to use MetricType and Value
        public async Task<GrowthAnalysisDto> CalculateGrowthRatesAsync(int startupId)
        {
            var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var lastMonthStart = currentMonthStart.AddMonths(-1);
            var oneYearAgoStart = currentMonthStart.AddYears(-1);

            var metrics = await _context.FinancialMetrics
                .Where(fm => fm.StartupId == startupId && !fm.IsArchived && fm.Date >= oneYearAgoStart)
                .OrderBy(fm => fm.Date)
                .ToListAsync();

            var currentMonthMetrics = metrics.Where(m => m.Date >= currentMonthStart);
            var lastMonthMetrics = metrics.Where(m => m.Date >= lastMonthStart && m.Date < currentMonthStart);

            var currentMonthRevenue = currentMonthMetrics.Where(m => m.MetricType == "Revenue").Sum(m => m.Value);
            var lastMonthRevenue = lastMonthMetrics.Where(m => m.MetricType == "Revenue").Sum(m => m.Value);
            
            var currentMonthSales = currentMonthMetrics.Where(m => m.MetricType == "SalesUnits").Sum(m => m.Value);
            var lastMonthSales = lastMonthMetrics.Where(m => m.MetricType == "SalesUnits").Sum(m => m.Value);

            // Calculate monthly growth rates
            double monthlyRevenueGrowth = (lastMonthRevenue == 0) ? (currentMonthRevenue > 0 ? 100.0 : 0.0) : (((double)currentMonthRevenue - (double)lastMonthRevenue) / (double)lastMonthRevenue * 100.0);
            double monthlySalesGrowth = (lastMonthSales == 0) ? (currentMonthSales > 0 ? 100.0 : 0.0) : (((double)currentMonthSales - (double)lastMonthSales) / (double)lastMonthSales * 100.0);

            // Calculate quarterly and yearly trends
            var quarterlyData = metrics
                .GroupBy(m => new { Quarter = (m.Date.Month - 1) / 3 + 1, m.Date.Year })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Quarter)
                .Select(g => new QuarterlyDataDto
                {
                    Year = g.Key.Year,
                    Quarter = g.Key.Quarter,
                    Revenue = g.Where(m => m.MetricType == "Revenue").Sum(m => m.Value),
                    Sales = g.Where(m => m.MetricType == "SalesUnits").Sum(m => m.Value)
                })
                .ToList();

            return new GrowthAnalysisDto
            {
                StartupId = startupId,
                MonthlyRevenueGrowth = monthlyRevenueGrowth,
                MonthlySalesGrowth = monthlySalesGrowth,
                QuarterlyData = quarterlyData
            };
        }
    }
}

