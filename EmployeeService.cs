using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SDMS.Domain.Entities;
using SDMS.Core.DTOs;
using System.Linq;
using SDMS.Infrastructure.Data;

namespace SDMS.Core.Services
{
    public class EmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(int startupId)
        {
            var employees = await _context.Employees
                .Include(e => e.User)
                .Where(e => e.StartupId == startupId)
                .ToListAsync();

            return employees.Select(e => new EmployeeDto
            {
                Id = e.Id,
                Username = e.User?.Username ?? "N/A", // Handle potential null User
                Email = e.User?.Email ?? "N/A",
                PhoneNumber = e.User?.PhoneNumber,
                EmployeeRole = e.EmployeeRole,
                PerformanceScore = e.PerformanceScore,
                HireDate = e.HireDate,
                StartupId = e.StartupId,
                Position = e.Position, // Added Position
                Salary = e.Salary, // Added Salary
                CommissionRate = e.CommissionRate // Added CommissionRate
            });
        }

        public async Task<EmployeeDto> GetEmployeeAsync(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                return null;

            return new EmployeeDto
            {
                Id = employee.Id,
                Username = employee.User?.Username ?? "N/A", // Handle potential null User
                Email = employee.User?.Email ?? "N/A",
                PhoneNumber = employee.User?.PhoneNumber,
                EmployeeRole = employee.EmployeeRole,
                PerformanceScore = employee.PerformanceScore,
                HireDate = employee.HireDate,
                StartupId = employee.StartupId,
                Position = employee.Position, // Added Position
                Salary = employee.Salary, // Added Salary
                CommissionRate = employee.CommissionRate // Added CommissionRate
            };
        }

        public async Task<bool> UpdateEmployeeAsync(int id, EmployeeUpdateDto dto)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return false;

            employee.EmployeeRole = dto.EmployeeRole;
            employee.PerformanceScore = dto.PerformanceScore;
            employee.Position = dto.Position; // Update Position
            employee.Salary = dto.Salary; // Update Salary
            employee.CommissionRate = dto.CommissionRate; // Update CommissionRate

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null || employee.User == null)
                return false;

            employee.User.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TrainingDto>> GetEmployeeTrainingsAsync(int employeeId)
        {
            var trainings = await _context.Trainings
                .Where(t => t.EmployeeId == employeeId)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            return trainings.Select(t => new TrainingDto
            {
                Id = t.Id,
                EmployeeId = t.EmployeeId,
                TrainingName = t.TrainingName,
                Description = t.Description,
                TrainingType = t.TrainingType,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                Status = t.Status,
                CompletionPercentage = t.CompletionPercentage,
                Feedback = t.Feedback
            });
        }

        public async Task<TrainingDto> GetTrainingAsync(int id)
        {
            var training = await _context.Trainings
                .FirstOrDefaultAsync(t => t.Id == id);

            if (training == null)
                return null;

            return new TrainingDto
            {
                Id = training.Id,
                EmployeeId = training.EmployeeId,
                TrainingName = training.TrainingName,
                Description = training.Description,
                TrainingType = training.TrainingType,
                StartDate = training.StartDate,
                EndDate = training.EndDate,
                Status = training.Status,
                CompletionPercentage = training.CompletionPercentage,
                Feedback = training.Feedback
            };
        }

        public async Task<TrainingDto> CreateTrainingAsync(TrainingCreateDto dto)
        {
            var training = new Training
            {
                EmployeeId = dto.EmployeeId,
                TrainingName = dto.TrainingName,
                Description = dto.Description,
                TrainingType = dto.TrainingType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = "Pending",
                CompletionPercentage = 0
            };

            _context.Trainings.Add(training);
            await _context.SaveChangesAsync();

            return new TrainingDto
            {
                Id = training.Id,
                EmployeeId = training.EmployeeId,
                TrainingName = training.TrainingName,
                Description = training.Description,
                TrainingType = training.TrainingType,
                StartDate = training.StartDate,
                EndDate = training.EndDate,
                Status = training.Status,
                CompletionPercentage = training.CompletionPercentage,
                Feedback = training.Feedback
            };
        }

        public async Task<bool> UpdateTrainingAsync(int id, TrainingUpdateDto dto)
        {
            var training = await _context.Trainings.FindAsync(id);
            if (training == null)
                return false;

            training.Status = dto.Status;
            training.CompletionPercentage = dto.CompletionPercentage;
            training.Feedback = dto.Feedback;
            
            if (dto.Status == "Completed" && !training.EndDate.HasValue)
            {
                training.EndDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PerformanceSummaryDto> GetPerformanceSummaryAsync(int employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null || employee.User == null)
                return null;

            var trainings = await _context.Trainings
                .Where(t => t.EmployeeId == employeeId)
                .ToListAsync();

            var completedTrainings = trainings.Count(t => t.Status == "Completed");
            var ongoingTrainings = trainings.Count(t => t.Status == "InProgress");
            var avgCompletion = trainings.Any() ? trainings.Average(t => t.CompletionPercentage) : 0;

            // Handle nullable HireDate for Tenure calculation
            int tenureInDays = 0;
            if (employee.HireDate.HasValue)
            {
                tenureInDays = (int)(DateTime.UtcNow - employee.HireDate.Value).TotalDays;
            }

            return new PerformanceSummaryDto
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.User.Username,
                EmployeeRole = employee.EmployeeRole,
                PerformanceScore = employee.PerformanceScore,
                HireDate = employee.HireDate,
                TenureInDays = tenureInDays, // Use calculated value
                CompletedTrainings = completedTrainings,
                OngoingTrainings = ongoingTrainings,
                AvgTrainingCompletion = avgCompletion
            };
        }

        public async Task<TeamPerformanceDto> GetTeamPerformanceAsync(int startupId)
        {
            var employees = await _context.Employees
                .Include(e => e.User)
                .Where(e => e.StartupId == startupId && e.User != null && e.User.IsActive)
                .ToListAsync();

            if (!employees.Any())
                return new TeamPerformanceDto
                {
                    StartupId = startupId,
                    EmployeeCount = 0,
                    AveragePerformance = 0,
                    TopPerformers = new List<EmployeeDto>(),
                    RoleDistribution = new Dictionary<string, int>()
                };

            var employeeDtos = employees.Select(e => new EmployeeDto
            {
                Id = e.Id,
                Username = e.User.Username,
                Email = e.User.Email,
                EmployeeRole = e.EmployeeRole,
                PerformanceScore = e.PerformanceScore,
                HireDate = e.HireDate,
                StartupId = e.StartupId,
                Position = e.Position, // Added Position
                Salary = e.Salary, // Added Salary
                CommissionRate = e.CommissionRate // Added CommissionRate
            }).ToList();

            var roleDistribution = employees
                .GroupBy(e => e.EmployeeRole)
                .ToDictionary(g => g.Key, g => g.Count());

            return new TeamPerformanceDto
            {
                StartupId = startupId,
                EmployeeCount = employees.Count,
                AveragePerformance = employees.Average(e => e.PerformanceScore),
                TopPerformers = employeeDtos
                    .OrderByDescending(e => e.PerformanceScore)
                    .Take(3)
                    .ToList(),
                RoleDistribution = roleDistribution
            };
        }
    }
}

