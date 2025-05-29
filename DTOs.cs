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
        public string Name { get; set; } // Added Name
        public string? PhoneNumber { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } // Added UpdatedAt
        public bool IsActive { get; set; }
    }

    public class UserCreateDto // Base for specific user types
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Name { get; set; } // Added Name
        public string Password { get; set; }
        public string? PhoneNumber { get; set; }
        // Role is set by specific registration endpoint
    }

    public class UserUpdateDto // Used for general profile updates
    {
        public string? Email { get; set; }
        public string? Name { get; set; } // Added Name
        public string? PhoneNumber { get; set; }
        public bool? IsActive { get; set; } // Allow updating active status (e.g., by admin)
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
    public class AdminDto : UserDto // Inherits common user fields
    {
        // Admin-specific fields
        public string? Department { get; set; }
        public string? Permissions { get; set; } // Example specific field
    }

    public class AdminCreateDto : UserCreateDto
    {
        // Admin-specific fields for creation
        public string? Department { get; set; }
        public string? Permissions { get; set; }
    }

    // StartupFounder DTOs
    public class StartupFounderDto : UserDto // Inherits common user fields
    {
        // Founder-specific fields
        public string? Bio { get; set; }
        public string? LinkedInProfile { get; set; }
    }

    public class StartupFounderCreateDto : UserCreateDto
    {
        // Founder-specific fields for creation
        public string? Bio { get; set; }
        public string? LinkedInProfile { get; set; }
    }

    // Employee DTOs
    public class EmployeeDto // Represents an Employee linked to a User
    {
        public int Id { get; set; } // Employee entity Id
        public int UserId { get; set; }
        public string Username { get; set; } // From related User
        public string Email { get; set; } // From related User
        public string Name { get; set; } // From related User
        public string? PhoneNumber { get; set; } // Added PhoneNumber from User
        public int StartupId { get; set; }
        public string? Position { get; set; }
        public decimal? Salary { get; set; }
        public decimal? CommissionRate { get; set; }
        public DateTime? HireDate { get; set; }
        public string? EmployeeRole { get; set; } // Added from previous version
        public float PerformanceScore { get; set; } // Added from previous version
    }

    public class EmployeeCreateDto : UserCreateDto
    {
        // Employee-specific fields for creation
        public int StartupId { get; set; }
        public string? Position { get; set; }
        public decimal? Salary { get; set; }
        public decimal? CommissionRate { get; set; }
        public DateTime? HireDate { get; set; }
        public string? EmployeeRole { get; set; } // Added from previous version
    }

    public class EmployeeUpdateDto // For updating Employee-specific details
    {
        public string? Position { get; set; }
        public decimal? Salary { get; set; }
        public decimal? CommissionRate { get; set; }
        public DateTime? HireDate { get; set; }
        public string? EmployeeRole { get; set; } // Added from previous version
        public float PerformanceScore { get; set; } // Added from previous version
    }

    // Startup DTOs
    public class StartupDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Industry { get; set; }
        public string? Stage { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? LogoUrl { get; set; }
        public int FounderId { get; set; }
        public string FounderName { get; set; } // Denormalized for convenience
        public DateTime? FoundedDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? SubscriptionStatus { get; set; }
    }

    public class StartupCreateDto
    {
        public string Name { get; set; }
        public string? Industry { get; set; }
        public string? Stage { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? LogoUrl { get; set; }
        public int FounderId { get; set; } // Should be set based on logged-in user or admin action
        public DateTime? FoundedDate { get; set; }
    }

    public class StartupUpdateDto
    {
        public string? Name { get; set; }
        public string? Industry { get; set; }
        public string? Stage { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? LogoUrl { get; set; }
        public DateTime? FoundedDate { get; set; }
    }

    // FinancialMetric DTOs (Refactored)
    public class FinancialMetricDto
    {
        public int Id { get; set; }
        public int StartupId { get; set; }
        public string MetricType { get; set; }
        public decimal Value { get; set; } // Changed to decimal
        public DateTime Date { get; set; }
        public string? Period { get; set; }
        public string? Notes { get; set; }
    }

    public class FinancialMetricCreateDto
    {
        public int StartupId { get; set; }
        public string MetricType { get; set; }
        public decimal Value { get; set; } // Changed to decimal
        public DateTime? Date { get; set; } // Changed to nullable DateTime
        public string? Period { get; set; }
        public string? Notes { get; set; }
    }

    public class FinancialMetricUpdateDto
    {
        public string? MetricType { get; set; }
        public decimal? Value { get; set; } // Changed to decimal, nullable
        public DateTime? Date { get; set; } // Added nullable Date
        public string? Period { get; set; }
        public string? Notes { get; set; }
    }

    public class FinancialSummaryDto
    {
        public int StartupId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; } // Changed to decimal
        public decimal TotalExpenses { get; set; } // Changed to decimal
        public decimal TotalSales { get; set; } // Changed to decimal (assuming units or value)
        public decimal NetProfit { get; set; } // Changed to decimal
        public int MetricsCount { get; set; }
        public List<MonthlyFinancialDataDto> MonthlyData { get; set; }
    }

    public class MonthlyFinancialDataDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; } // Changed to decimal
        public decimal Expenses { get; set; } // Changed to decimal
        public decimal Sales { get; set; } // Changed to decimal
        public decimal Profit { get; set; } // Changed to decimal
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
        public decimal Revenue { get; set; } // Changed to decimal
        public decimal Sales { get; set; } // Changed to decimal
    }

    // Subscription DTOs
    public class SubscriptionDto
    {
        public int Id { get; set; }
        public int StartupId { get; set; }
        public int PlanId { get; set; } // Added PlanId
        public string PlanName { get; set; } // Added PlanName
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PricePaid { get; set; } // Added PricePaid
        public bool IsActive { get; set; }
        // Removed fields not directly on Subscription entity (like AutoRenew, PaymentStatus)
    }

    public class SubscriptionCreateDto // Simplified for creating a subscription
    {
        public int StartupId { get; set; }
        public int PlanId { get; set; }
        // StartDate, EndDate, PricePaid, IsActive usually determined by service logic
    }

    public class SubscriptionUpdateDto // Example: Maybe only IsActive can be updated?
    {
        public bool IsActive { get; set; }
    }

    // Training DTOs
    public class TrainingDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string CourseName { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? CertificateUrl { get; set; }
        public string? TrainingName { get; set; } // Added from previous version
        public string? Description { get; set; } // Added from previous version
        public string? TrainingType { get; set; } // Added from previous version
        public DateTime StartDate { get; set; } // Added from previous version
        public DateTime? EndDate { get; set; } // Added from previous version
        public string? Status { get; set; } // Added from previous version
        public float CompletionPercentage { get; set; } // Added from previous version
        public string? Feedback { get; set; } // Added from previous version
    }

    public class TrainingCreateDto
    {
        public int EmployeeId { get; set; }
        public string CourseName { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? CertificateUrl { get; set; }
        public string? TrainingName { get; set; } // Added from previous version
        public string? Description { get; set; } // Added from previous version
        public string? TrainingType { get; set; } // Added from previous version
        public DateTime StartDate { get; set; } // Added from previous version
        public DateTime? EndDate { get; set; } // Added from previous version
    }

    public class TrainingUpdateDto
    {
        public string? CourseName { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? CertificateUrl { get; set; }
        public string? Status { get; set; } // Added from previous version
        public float CompletionPercentage { get; set; } // Added from previous version
        public string? Feedback { get; set; } // Added from previous version
    }

    // Notification DTOs
    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; } // Added from previous version
        public string? NotificationType { get; set; } // Added from previous version
        public string? DeliveryStatus { get; set; } // Added from previous version
    }

    public class NotificationCreateDto
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; } // Added from previous version
        public string? NotificationType { get; set; } // Added from previous version
    }

    public class BulkNotificationCreateDto
    {
        public List<int> UserIds { get; set; }
        public string Message { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; } // Added from previous version
        public string? NotificationType { get; set; } // Added from previous version
    }

    // AuditLog DTOs
    public class AuditLogDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; } // Denormalized
        public string Action { get; set; }
        public string? EntityName { get; set; }
        public string? EntityId { get; set; }
        public string? Changes { get; set; } // JSON string or similar
        public DateTime Timestamp { get; set; }
        public string? OldValues { get; set; } // Added from previous version
        public string? NewValues { get; set; } // Added from previous version
        public string? IpAddress { get; set; } // Added from previous version
    }

    // Report DTOs
    public class ReportDto
    {
        public int Id { get; set; }
        public int? StartupId { get; set; }
        public int GeneratedById { get; set; }
        public string GeneratedByName { get; set; } // Denormalized
        public string ReportType { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string FilePath { get; set; }
        public string? Parameters { get; set; } // JSON string or similar
        public string? Title { get; set; } // Added from previous version
        public string? Description { get; set; } // Added from previous version
        public string? GeneratedBy { get; set; } // Added from previous version
        public string? Format { get; set; } // Added from previous version
        public string? StoragePath { get; set; } // Added from previous version
    }

    public class ReportGenerateDto // Input for generating a report
    {
        public int? StartupId { get; set; }
        public string ReportType { get; set; }
        public string Format { get; set; } // e.g., "pdf", "excel"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int GeneratedById { get; set; } // Added from previous version
        // Add other parameters as needed
    }

    // Authentication DTOs
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public UserDto User { get; set; } // Added UserDto object
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
        private const int MaxPageSize = 100; // Increased max page size
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : (value < 1 ? 1 : value); // Ensure positive page size
        }
    }

    // Template DTOs
    public class TemplateDto
    {
        public int Id { get; set; } // Use int ID from DB
        public string TemplateIdentifier { get; set; } // Keep identifier if needed
        public string Name { get; set; }
        public string Description { get; set; }
    }

    // DTO for updating user profile
    public class UserProfileUpdateDto
    {
        public string? Name { get; set; } 
        public string? Email { get; set; } 
        public string? PhoneNumber { get; set; } // Added phone number
        // Add other updatable profile fields as needed, e.g., bio for founder
    }

    // DTO for Subscription Plans (Renamed from PlanDto)
    public class SubscriptionPlanDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? MemberLimit { get; set; }
        public decimal PricePerMember { get; set; } // Use decimal
        public string? PriceDescription { get; set; } // e.g., "Custom", "Free"
        public List<string> Features { get; set; } // Assuming stored as JSON and deserialized
        public string PlanType { get; set; } // Added from previous version
        public decimal MonthlyCost { get; set; } // Added from previous version
    }

    // Performance DTOs (Added based on build errors)
    public class PerformanceSummaryDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string? EmployeeRole { get; set; }
        public float PerformanceScore { get; set; }
        public DateTime? HireDate { get; set; }
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
}

