using System;
using System.Collections.Generic;

namespace SDMS.Core.DTOs
{
    // User DTOs
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserCreateDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
    }

    public class UserUpdateDto
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    // Admin DTOs
    public class AdminDto : UserDto
    {
        public string AdminLevel { get; set; }
        public string Department { get; set; }
    }

    public class AdminCreateDto : UserCreateDto
    {
        public string AdminLevel { get; set; }
        public string Department { get; set; }
    }

    public class AdminUpdateDto : UserUpdateDto
    {
        public string AdminLevel { get; set; }
        public string Department { get; set; }
    }

    // StartupFounder DTOs
    public class StartupFounderDto : UserDto
    {
        public string CompanyName { get; set; }
    }

    public class StartupFounderCreateDto : UserCreateDto
    {
        public string CompanyName { get; set; }
    }

    public class StartupFounderUpdateDto : UserUpdateDto
    {
        public string CompanyName { get; set; }
    }

    // Employee DTOs
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int StartupId { get; set; }
        public string EmployeeRole { get; set; }
        public float PerformanceScore { get; set; }
        public DateTime HireDate { get; set; }
    }

    public class EmployeeCreateDto : UserCreateDto
    {
        public int StartupId { get; set; }
        public string EmployeeRole { get; set; }
    }

    public class EmployeeUpdateDto
    {
        public string EmployeeRole { get; set; }
        public float PerformanceScore { get; set; }
    }

    // Startup DTOs
    public class StartupDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Industry { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public DateTime FoundingDate { get; set; }
        public int FounderId { get; set; }
        public string SubscriptionStatus { get; set; }
        public string FounderName { get; set; }
    }

    public class StartupCreateDto
    {
        public string Name { get; set; }
        public string Industry { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public DateTime? FoundingDate { get; set; }
        public int FounderId { get; set; }
    }

    public class StartupUpdateDto
    {
        public string Name { get; set; }
        public string Industry { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
    }

    // FinancialMetric DTOs
    public class FinancialMetricDto
    {
        public int Id { get; set; }
        public int StartupId { get; set; }
        public DateTime Date { get; set; }
        public float Revenue { get; set; }
        public float Expenses { get; set; }
        public float MonthlySales { get; set; }
        public float Profit { get; set; }
        public string Notes { get; set; }
    }

    public class FinancialMetricCreateDto
    {
        public int StartupId { get; set; }
        public DateTime Date { get; set; }
        public float Revenue { get; set; }
        public float Expenses { get; set; }
        public float MonthlySales { get; set; }
        public string Notes { get; set; }
    }

    public class FinancialMetricUpdateDto
    {
        public float Revenue { get; set; }
        public float Expenses { get; set; }
        public float MonthlySales { get; set; }
        public string Notes { get; set; }
    }

    public class FinancialSummaryDto
    {
        public int StartupId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public float TotalRevenue { get; set; }
        public float TotalExpenses { get; set; }
        public float TotalSales { get; set; }
        public float NetProfit { get; set; }
        public int MetricsCount { get; set; }
        public List<MonthlyFinancialDataDto> MonthlyData { get; set; }
    }

    public class MonthlyFinancialDataDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public float Revenue { get; set; }
        public float Expenses { get; set; }
        public float Sales { get; set; }
        public float Profit { get; set; }
    }

    public class GrowthAnalysisDto
    {
        public int StartupId { get; set; }
        public double MonthlyRevenueGrowth { get; set; }
        public double MonthlySalesGrowth { get; set; }
        public List<QuarterlyDataDto> QuarterlyData { get; set; }
    }

    public class QuarterlyDataDto
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        public float Revenue { get; set; }
        public float Sales { get; set; }
    }

    // Subscription DTOs
    public class SubscriptionDto
    {
        public int Id { get; set; }
        public int StartupId { get; set; }
        public string PlanType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Cost { get; set; }
        public bool IsActive { get; set; }
        public bool AutoRenew { get; set; }
        public string PaymentStatus { get; set; }
        public Dictionary<string, decimal> CostBreakdown { get; set; }
    }

    public class SubscriptionCreateDto
    {
        public int StartupId { get; set; }
        public string PlanType { get; set; }
        public int DurationMonths { get; set; }
        public decimal Cost { get; set; }
        public bool AutoRenew { get; set; }
    }

    public class SubscriptionUpdateDto
    {
        public bool AutoRenew { get; set; }
        public string PaymentStatus { get; set; }
    }

    public class SubscriptionPlanDto
    {
        public string PlanType { get; set; }
        public decimal MonthlyCost { get; set; }
        public List<string> Features { get; set; }
    }

    // Training DTOs
    public class TrainingDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string TrainingName { get; set; }
        public string Description { get; set; }
        public string TrainingType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public float CompletionPercentage { get; set; }
        public string Feedback { get; set; }
    }

    public class TrainingCreateDto
    {
        public int EmployeeId { get; set; }
        public string TrainingName { get; set; }
        public string Description { get; set; }
        public string TrainingType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class TrainingUpdateDto
    {
        public string Status { get; set; }
        public float CompletionPercentage { get; set; }
        public string Feedback { get; set; }
    }

    // Notification DTOs
    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string NotificationType { get; set; }
        public string DeliveryStatus { get; set; }
    }

    public class NotificationCreateDto
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
    }

    public class BulkNotificationCreateDto
    {
        public List<int> UserIds { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
    }

    // AuditLog DTOs
    public class AuditLogDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string EntityName { get; set; }
        public int? EntityId { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
    }

    // Report DTOs
    public class ReportDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
        public string ReportType { get; set; }
        public string Format { get; set; }
        public string StoragePath { get; set; }
        public int? StartupId { get; set; }
    }

    public class ReportGenerateDto
    {
        public int StartupId { get; set; }
        public int GeneratedById { get; set; }
        public string Format { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    // Performance DTOs
    public class PerformanceSummaryDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeRole { get; set; }
        public float PerformanceScore { get; set; }
        public DateTime HireDate { get; set; }
        public int TenureInDays { get; set; }
        public int CompletedTrainings { get; set; }
        public int OngoingTrainings { get; set; }
        public double AvgTrainingCompletion { get; set; }
    }

    public class TeamPerformanceDto
    {
        public int StartupId { get; set; }
        public int EmployeeCount { get; set; }
        public double AveragePerformance { get; set; }
        public List<EmployeeDto> TopPerformers { get; set; }
        public Dictionary<string, int> RoleDistribution { get; set; }
    }

    // Authentication DTOs
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime Expiration { get; set; }
    }

    // Pagination DTOs
    public class PaginatedResultDto<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class PaginationParametersDto
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }
    }
}
