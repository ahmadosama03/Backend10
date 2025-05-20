using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SDMS.Domain.Entities;
using SDMS.Core.DTOs;
using System.Linq;
using System.Text.Json;
using SDMS.Infrastructure.Data;// Add this line or update with the correct namespace for ApplicationDbContext

namespace SDMS.Core.Services
{
    public class FinancialService
    {
        private readonly ApplicationDbContext _context;

        public FinancialService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FinancialMetricDto>> GetFinancialMetricsAsync(int startupId)
        {
            var metrics = await _context.FinancialMetrics
                .Where(fm => fm.StartupId == startupId && !fm.IsArchived)
                .OrderByDescending(fm => fm.Date)
                .ToListAsync();

            return metrics.Select(m => new FinancialMetricDto
            {
                Id = m.Id,
                StartupId = m.StartupId,
                Date = m.Date,
                Revenue = m.Revenue,
                Expenses = m.Expenses,
                MonthlySales = m.MonthlySales,
                Profit = m.Revenue - m.Expenses,
                Notes = m.Notes
            });
        }

        public async Task<FinancialMetricDto> GetFinancialMetricAsync(int id)
        {
            var metric = await _context.FinancialMetrics
                .FirstOrDefaultAsync(fm => fm.Id == id);

            if (metric == null)
                return null;

            return new FinancialMetricDto
            {
                Id = metric.Id,
                StartupId = metric.StartupId,
                Date = metric.Date,
                Revenue = metric.Revenue,
                Expenses = metric.Expenses,
                MonthlySales = metric.MonthlySales,
                Profit = metric.Revenue - metric.Expenses,
                Notes = metric.Notes
            };
        }

        public async Task<FinancialMetricDto> CreateFinancialMetricAsync(FinancialMetricCreateDto dto)
        {
            var metric = new FinancialMetric
            {
                StartupId = dto.StartupId,
                Date = dto.Date,
                Revenue = dto.Revenue,
                Expenses = dto.Expenses,
                MonthlySales = dto.MonthlySales,
                Notes = dto.Notes
            };

            _context.FinancialMetrics.Add(metric);
            await _context.SaveChangesAsync();

            return new FinancialMetricDto
            {
                Id = metric.Id,
                StartupId = metric.StartupId,
                Date = metric.Date,
                Revenue = metric.Revenue,
                Expenses = metric.Expenses,
                MonthlySales = metric.MonthlySales,
                Profit = metric.Revenue - metric.Expenses,
                Notes = metric.Notes
            };
        }

        public async Task<bool> UpdateFinancialMetricAsync(int id, FinancialMetricUpdateDto dto)
        {
            var metric = await _context.FinancialMetrics.FindAsync(id);
            if (metric == null)
                return false;

            metric.Revenue = dto.Revenue;
            metric.Expenses = dto.Expenses;
            metric.MonthlySales = dto.MonthlySales;
            metric.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFinancialMetricAsync(int id)
        {
            var metric = await _context.FinancialMetrics.FindAsync(id);
            if (metric == null)
                return false;

            metric.IsArchived = true;
            await _context.SaveChangesAsync();
            return true;
        }

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
                    TotalSales = 0,
                    NetProfit = 0,
                    MetricsCount = 0
                };
            }

            var summary = new FinancialSummaryDto
            {
                StartupId = startupId,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalRevenue = metrics.Sum(m => m.Revenue),
                TotalExpenses = metrics.Sum(m => m.Expenses),
                TotalSales = metrics.Sum(m => m.MonthlySales),
                NetProfit = metrics.Sum(m => m.Revenue - m.Expenses),
                MetricsCount = metrics.Count,
                MonthlyData = metrics
                    .GroupBy(m => new { m.Date.Year, m.Date.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .Select(g => new MonthlyFinancialDataDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Revenue = g.Sum(m => m.Revenue),
                        Expenses = g.Sum(m => m.Expenses),
                        Sales = g.Sum(m => m.MonthlySales),
                        Profit = g.Sum(m => m.Revenue - m.Expenses)
                    })
                    .ToList()
            };

            return summary;
        }

        public async Task<GrowthAnalysisDto> CalculateGrowthRatesAsync(int startupId)
        {
            var currentMonth = DateTime.UtcNow;
            var lastMonth = currentMonth.AddMonths(-1);
            var threeMonthsAgo = currentMonth.AddMonths(-3);
            var sixMonthsAgo = currentMonth.AddMonths(-6);
            var oneYearAgo = currentMonth.AddYears(-1);

            var metrics = await _context.FinancialMetrics
                .Where(fm => fm.StartupId == startupId && !fm.IsArchived)
                .OrderBy(fm => fm.Date)
                .ToListAsync();

            var currentMonthMetrics = metrics.Where(m => m.Date.Month == currentMonth.Month && m.Date.Year == currentMonth.Year);
            var lastMonthMetrics = metrics.Where(m => m.Date.Month == lastMonth.Month && m.Date.Year == lastMonth.Year);
            var threeMonthsAgoMetrics = metrics.Where(m => m.Date >= threeMonthsAgo);
            var sixMonthsAgoMetrics = metrics.Where(m => m.Date >= sixMonthsAgo);
            var oneYearAgoMetrics = metrics.Where(m => m.Date >= oneYearAgo);

            var currentMonthRevenue = currentMonthMetrics.Sum(m => m.Revenue);
            var lastMonthRevenue = lastMonthMetrics.Sum(m => m.Revenue);
            
            var currentMonthSales = currentMonthMetrics.Sum(m => m.MonthlySales);
            var lastMonthSales = lastMonthMetrics.Sum(m => m.MonthlySales);

            // Calculate monthly growth rates
            double monthlyRevenueGrowth = 0;
            if (lastMonthRevenue > 0)
            {
                monthlyRevenueGrowth = ((double)currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue * 100;
            }

            double monthlySalesGrowth = 0;
            if (lastMonthSales > 0)
            {
                monthlySalesGrowth = ((double)currentMonthSales - lastMonthSales) / lastMonthSales * 100;
            }

            // Calculate quarterly and yearly trends
            var quarterlyData = metrics
                .Where(m => m.Date >= oneYearAgo)
                .GroupBy(m => new { Quarter = (m.Date.Month - 1) / 3 + 1, m.Date.Year })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Quarter)
                .Select(g => new QuarterlyDataDto
                {
                    Year = g.Key.Year,
                    Quarter = g.Key.Quarter,
                    Revenue = g.Sum(m => m.Revenue),
                    Sales = g.Sum(m => m.MonthlySales)
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
