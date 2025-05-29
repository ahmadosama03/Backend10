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

    // Base validator for common user creation fields
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

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Invalid phone number format")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            // Removed Role validation as it's determined by the endpoint
            // RuleFor(x => x.Role).NotEmpty()... 
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

    // Validator for Admin creation, inheriting base user validation
    public class AdminCreateDtoValidator : AbstractValidator<AdminCreateDto>
    {
        public AdminCreateDtoValidator()
        {
            Include(new UserCreateDtoValidator());

            // Removed AdminLevel validation as it's not in the DTO
            // RuleFor(x => x.AdminLevel).NotEmpty()...

            RuleFor(x => x.Department)
                .MaximumLength(50).WithMessage("Department must not exceed 50 characters");

            RuleFor(x => x.Permissions)
                .MaximumLength(255).WithMessage("Permissions must not exceed 255 characters");
        }
    }

    // Validator for Startup Founder creation, inheriting base user validation
    public class StartupFounderCreateDtoValidator : AbstractValidator<StartupFounderCreateDto>
    {
        public StartupFounderCreateDtoValidator()
        {
            Include(new UserCreateDtoValidator());

            // Removed CompanyName validation as it's not in the DTO
            // RuleFor(x => x.CompanyName).NotEmpty()...

            RuleFor(x => x.Bio)
                .MaximumLength(1000).WithMessage("Bio must not exceed 1000 characters");

            RuleFor(x => x.LinkedInProfile)
                .MaximumLength(255).WithMessage("LinkedIn profile URL must not exceed 255 characters");
        }
    }

    // Validator for Employee creation, inheriting base user validation
    public class EmployeeCreateDtoValidator : AbstractValidator<EmployeeCreateDto>
    {
        public EmployeeCreateDtoValidator()
        {
            Include(new UserCreateDtoValidator());

            RuleFor(x => x.StartupId)
                .GreaterThan(0).WithMessage("Startup ID must be greater than 0");

            RuleFor(x => x.Position)
                .MaximumLength(100).WithMessage("Position must not exceed 100 characters");

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0).When(x => x.Salary.HasValue).WithMessage("Salary must be non-negative");

            RuleFor(x => x.CommissionRate)
                .InclusiveBetween(0, 1).When(x => x.CommissionRate.HasValue).WithMessage("Commission rate must be between 0 and 1");

            // Removed EmployeeRole validation as it's not in the DTO
            // RuleFor(x => x.EmployeeRole).NotEmpty()...
        }
    }
}

