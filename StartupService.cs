using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SDMS.Core.DTOs;
using SDMS.Domain.Entities;
using SDMS.Infrastructure.Data; // Assuming DbContext is here

namespace SDMS.Core.Services
{
    public class StartupService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public StartupService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<StartupDto>> GetStartupsAsync()
        {
            var startups = await _context.Startups.ToListAsync(); // Assuming Startups DbSet
            return _mapper.Map<IEnumerable<StartupDto>>(startups);
        }

        public async Task<StartupDto> GetStartupAsync(int id)
        {
            var startup = await _context.Startups.FindAsync(id);
            return _mapper.Map<StartupDto>(startup);
        }

        public async Task<IEnumerable<StartupDto>> GetStartupsByFounderAsync(int founderId)
        {
            // Assuming a relationship between User (Founder) and Startup
            var startups = await _context.Startups
                                       .Where(s => s.FounderId == founderId) // Assuming FounderId property
                                       .ToListAsync();
            return _mapper.Map<IEnumerable<StartupDto>>(startups);
        }

        public async Task<StartupDto> CreateStartupAsync(StartupCreateDto dto)
        {
            var startup = _mapper.Map<Startup>(dto);
            startup.CreatedAt = DateTime.UtcNow;
            // Set FounderId based on authenticated user or DTO
            // startup.FounderId = ...; 

            _context.Startups.Add(startup);
            await _context.SaveChangesAsync();
            return _mapper.Map<StartupDto>(startup);
        }

        public async Task<bool> UpdateStartupAsync(int id, StartupUpdateDto dto)
        {
            var startup = await _context.Startups.FindAsync(id);
            if (startup == null)
                return false;

            _mapper.Map(dto, startup);
            startup.UpdatedAt = DateTime.UtcNow; // Use the added UpdatedAt property

            _context.Startups.Update(startup);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteStartupAsync(int id)
        {
            var startup = await _context.Startups.FindAsync(id);
            if (startup == null)
                return false;

            _context.Startups.Remove(startup);
            await _context.SaveChangesAsync();
            return true;
        }

        // --- Updated Dashboard Data Logic ---
        public async Task<object> GetStartupDashboardAsync(int id)
        {
            var startup = await _context.Startups.FindAsync(id);
            if (startup == null) return null;

            // Fetch relevant financial data for the startup
            var financialData = await _context.FinancialMetrics
                                            .Where(fm => fm.StartupId == id)
                                            .OrderBy(fm => fm.Date) 
                                            .ToListAsync();

            // Structure data according to frontend needs (using corrected properties)
            var dashboardData = new
            {
                revenue = financialData.Where(f => f.MetricType == "Revenue").Select(f => new { month = f.Date.ToString("MMM"), revenue = f.Value }).ToList(),
                expenses = financialData.Where(f => f.MetricType == "Expense").Select(f => new { month = f.Date.ToString("MMM"), amount = f.Value }).ToList(),
                // Placeholder for customers - needs a real data source/entity
                customers = new [] { new { month = "Jun", count = 1250 } }, 
                metrics = new { 
                    revenue = new { monthly = financialData.Where(f => f.MetricType == "Revenue").Select(f => f.Value).ToList() },
                    // Placeholder for customer metrics
                    customers = new { count = 1250, growth = 15.3 }, 
                    // Placeholder for project metrics
                    projects = new { count = 8, growth = 3.2 }, 
                    expenses = new { monthly = financialData.Where(f => f.MetricType == "Expense").Select(f => f.Value).ToList() }
                }
            };

            return dashboardData;
        }

        // --- Transfer Ownership Logic ---
        public async Task<bool> TransferOwnershipAsync(int startupId, int newFounderUserId)
        {
            var startup = await _context.Startups.FindAsync(startupId);
            if (startup == null)
            {
                return false; // Startup not found
            }

            var newFounder = await _context.Users.FindAsync(newFounderUserId);
            // Assuming Role property exists on User entity
            if (newFounder == null || newFounder.Role != "StartupFounder") // Ensure new owner is a founder
            {
                return false; // New founder not valid
            }

            // Update the founder reference on the startup
            startup.FounderId = newFounderUserId; 
            startup.UpdatedAt = DateTime.UtcNow; // Update timestamp

            _context.Startups.Update(startup);
            await _context.SaveChangesAsync();

            // Optional: Add logic to update roles, permissions, notifications etc.

            return true;
        }
    }
}

