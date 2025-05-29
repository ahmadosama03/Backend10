using FluentValidation;
using SDMS.Core.DTOs;
using System;

namespace SDMS.Core.Validation
{
    public class StartupCreateDtoValidator : AbstractValidator<StartupCreateDto>
    {
        public StartupCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Startup name is required")
                .Length(2, 100).WithMessage("Startup name must be between 2 and 100 characters");

            RuleFor(x => x.Industry)
                .MaximumLength(50).WithMessage("Industry must not exceed 50 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.FounderId)
                .GreaterThan(0).WithMessage("Founder ID must be greater than 0");
        }
    }

    // Updated Validator for the refactored DTO
    public class FinancialMetricCreateDtoValidator : AbstractValidator<FinancialMetricCreateDto>
    {
        public FinancialMetricCreateDtoValidator()
        {
            RuleFor(x => x.StartupId)
                .GreaterThan(0).WithMessage("Startup ID must be greater than 0");

            RuleFor(x => x.MetricType)
                .NotEmpty().WithMessage("Metric Type is required")
                .MaximumLength(50).WithMessage("Metric Type must not exceed 50 characters");

            RuleFor(x => x.Value)
                .NotNull().WithMessage("Value is required"); // Basic check, could add range checks depending on MetricType

            RuleFor(x => x.Date)
                .NotNull().WithMessage("Date is required") // Changed from NotEmpty for nullable DateTime
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date cannot be in the future");

            RuleFor(x => x.Period)
                .MaximumLength(20).WithMessage("Period must not exceed 20 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");
        }
    }

    // Updated Validator for the refactored DTO
    public class SubscriptionCreateDtoValidator : AbstractValidator<SubscriptionCreateDto>
    {
        public SubscriptionCreateDtoValidator()
        {
            RuleFor(x => x.StartupId)
                .GreaterThan(0).WithMessage("Startup ID must be greater than 0");

            RuleFor(x => x.PlanId)
                .GreaterThan(0).WithMessage("Plan ID must be greater than 0");
        }
    }

    // Updated Validator for the refactored DTO
    public class TrainingCreateDtoValidator : AbstractValidator<TrainingCreateDto>
    {
        public TrainingCreateDtoValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("Employee ID must be greater than 0");

            RuleFor(x => x.CourseName)
                .NotEmpty().WithMessage("Course name is required")
                .MaximumLength(100).WithMessage("Course name must not exceed 100 characters");

            // Removed validation for properties that no longer exist in the DTO
            // RuleFor(x => x.TrainingName).NotEmpty()... 
            // RuleFor(x => x.Description).MaximumLength(500)...
            // RuleFor(x => x.TrainingType).NotEmpty()...
            // RuleFor(x => x.StartDate).NotEmpty()...
            // RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)...
        }
    }

    // Updated Validator for the refactored DTO
    public class NotificationCreateDtoValidator : AbstractValidator<NotificationCreateDto>
    {
        public NotificationCreateDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("User ID must be greater than 0");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required")
                .MaximumLength(500).WithMessage("Message must not exceed 500 characters");

            RuleFor(x => x.Type)
                .MaximumLength(50).WithMessage("Type must not exceed 50 characters");

            // Removed validation for properties that no longer exist in the DTO
            // RuleFor(x => x.Title).NotEmpty()...
            // RuleFor(x => x.NotificationType).NotEmpty()...
        }
    }

    // Updated Validator for the refactored DTO
    public class ReportGenerateDtoValidator : AbstractValidator<ReportGenerateDto>
    {
        public ReportGenerateDtoValidator()
        {
            RuleFor(x => x.StartupId)
                .GreaterThan(0).When(x => x.StartupId.HasValue).WithMessage("Startup ID must be greater than 0 if provided");

            // Removed GeneratedById validation as it's not in the DTO
            // RuleFor(x => x.GeneratedById).GreaterThan(0)...

            RuleFor(x => x.ReportType)
                .NotEmpty().WithMessage("Report Type is required")
                .MaximumLength(50).WithMessage("Report Type must not exceed 50 characters");

            RuleFor(x => x.Format)
                .NotEmpty().WithMessage("Format is required")
                .Must(format => format.ToLower() == "pdf" || format.ToLower() == "excel")
                .WithMessage("Format must be PDF or Excel");

            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("End date must be after or equal to start date");
        }
    }
}

