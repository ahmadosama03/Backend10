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
                .NotEmpty().WithMessage("Industry is required")
                .MaximumLength(50).WithMessage("Industry must not exceed 50 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.FounderId)
                .GreaterThan(0).WithMessage("Founder ID must be greater than 0");
        }
    }

    public class FinancialMetricCreateDtoValidator : AbstractValidator<FinancialMetricCreateDto>
    {
        public FinancialMetricCreateDtoValidator()
        {
            RuleFor(x => x.StartupId)
                .GreaterThan(0).WithMessage("Startup ID must be greater than 0");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date cannot be in the future");

            RuleFor(x => x.Revenue)
                .GreaterThanOrEqualTo(0).WithMessage("Revenue must be greater than or equal to 0");

            RuleFor(x => x.Expenses)
                .GreaterThanOrEqualTo(0).WithMessage("Expenses must be greater than or equal to 0");

            RuleFor(x => x.MonthlySales)
                .GreaterThanOrEqualTo(0).WithMessage("Monthly sales must be greater than or equal to 0");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");
        }
    }

    public class SubscriptionCreateDtoValidator : AbstractValidator<SubscriptionCreateDto>
    {
        public SubscriptionCreateDtoValidator()
        {
            RuleFor(x => x.StartupId)
                .GreaterThan(0).WithMessage("Startup ID must be greater than 0");

            RuleFor(x => x.PlanType)
                .NotEmpty().WithMessage("Plan type is required")
                .Must(type => type == "Free" || type == "Pro" || type == "Growth" || type == "Enterprise")
                .WithMessage("Plan type must be Free, Pro, Growth, or Enterprise");

            RuleFor(x => x.DurationMonths)
                .GreaterThan(0).WithMessage("Duration must be greater than 0 months");

            RuleFor(x => x.Cost)
                .GreaterThanOrEqualTo(0).WithMessage("Cost must be greater than or equal to 0");
        }
    }

    public class TrainingCreateDtoValidator : AbstractValidator<TrainingCreateDto>
    {
        public TrainingCreateDtoValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("Employee ID must be greater than 0");

            RuleFor(x => x.TrainingName)
                .NotEmpty().WithMessage("Training name is required")
                .MaximumLength(100).WithMessage("Training name must not exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.TrainingType)
                .NotEmpty().WithMessage("Training type is required")
                .MaximumLength(50).WithMessage("Training type must not exceed 50 characters");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
                .When(x => x.EndDate.HasValue);
        }
    }

    public class NotificationCreateDtoValidator : AbstractValidator<NotificationCreateDto>
    {
        public NotificationCreateDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("User ID must be greater than 0");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(100).WithMessage("Title must not exceed 100 characters");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required")
                .MaximumLength(500).WithMessage("Message must not exceed 500 characters");

            RuleFor(x => x.NotificationType)
                .NotEmpty().WithMessage("Notification type is required")
                .Must(type => type == "Email" || type == "SMS" || type == "InApp")
                .WithMessage("Notification type must be Email, SMS, or InApp");
        }
    }

    public class ReportGenerateDtoValidator : AbstractValidator<ReportGenerateDto>
    {
        public ReportGenerateDtoValidator()
        {
            RuleFor(x => x.StartupId)
                .GreaterThan(0).WithMessage("Startup ID must be greater than 0");

            RuleFor(x => x.GeneratedById)
                .GreaterThan(0).WithMessage("Generated by ID must be greater than 0");

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
