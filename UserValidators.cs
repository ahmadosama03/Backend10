using FluentValidation;
using SDMS.Core.DTOs;
using System;

namespace SDMS.Core.Validation
{
    public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
    {
        public UserLoginDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .Length(3, 50).WithMessage("Username must be between 3 and 50 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters");
        }
    }

    public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
    {
        public UserCreateDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .Length(3, 50).WithMessage("Username must be between 3 and 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Invalid phone number format")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .Must(role => role == "Admin" || role == "StartupFounder" || role == "Employee")
                .WithMessage("Role must be Admin, StartupFounder, or Employee");
        }
    }

    public class UserChangePasswordDtoValidator : AbstractValidator<UserChangePasswordDto>
    {
        public UserChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .MinimumLength(6).WithMessage("New password must be at least 6 characters")
                .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter")
                .Matches("[0-9]").WithMessage("New password must contain at least one number");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
        }
    }

    public class AdminCreateDtoValidator : AbstractValidator<AdminCreateDto>
    {
        public AdminCreateDtoValidator()
        {
            Include(new UserCreateDtoValidator());

            RuleFor(x => x.AdminLevel)
                .NotEmpty().WithMessage("Admin level is required")
                .Must(level => level == "SuperAdmin" || level == "SystemAdmin" || level == "SupportAdmin")
                .WithMessage("Admin level must be SuperAdmin, SystemAdmin, or SupportAdmin");

            RuleFor(x => x.Department)
                .NotEmpty().WithMessage("Department is required")
                .MaximumLength(50).WithMessage("Department must not exceed 50 characters");
        }
    }

    public class StartupFounderCreateDtoValidator : AbstractValidator<StartupFounderCreateDto>
    {
        public StartupFounderCreateDtoValidator()
        {
            Include(new UserCreateDtoValidator());

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required")
                .MaximumLength(100).WithMessage("Company name must not exceed 100 characters");
        }
    }

    public class EmployeeCreateDtoValidator : AbstractValidator<EmployeeCreateDto>
    {
        public EmployeeCreateDtoValidator()
        {
            Include(new UserCreateDtoValidator());

            RuleFor(x => x.StartupId)
                .GreaterThan(0).WithMessage("Startup ID must be greater than 0");

            RuleFor(x => x.EmployeeRole)
                .NotEmpty().WithMessage("Employee role is required")
                .MaximumLength(50).WithMessage("Employee role must not exceed 50 characters");
        }
    }
}
