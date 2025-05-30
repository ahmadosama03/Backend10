using FluentValidation;
using SDMS.Core.DTOs;
using System;

namespace SDMS.Core.Validation
{
    // Validator for UserLoginDto (Updated)
    public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
    {
        public UserLoginDtoValidator()
        {
            RuleFor(x => x.Email) // Changed from Username
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters");
        }
    }

    // Base validator for common user creation fields (Keep for reference/other DTOs if needed, but review usage)
    // This might not be directly used by StartupFounderCreateDto anymore due to field differences.
    public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
    {
        public UserCreateDtoValidator()
        {
            // Keep Username validation if UserCreateDto is still used elsewhere and requires it.
            // If Username is always derived from Email now, this rule might be removed or changed.
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .Length(3, 100).WithMessage("Username must be between 3 and 100 characters"); // Adjusted length

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            // Keep Name validation if UserCreateDto is still used elsewhere.
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
            Include(new UserCreateDtoValidator()); // Assuming AdminCreateDto still inherits/matches UserCreateDto

            RuleFor(x => x.Department)
                .MaximumLength(50).WithMessage("Department must not exceed 50 characters");

            RuleFor(x => x.Permissions)
                .MaximumLength(255).WithMessage("Permissions must not exceed 255 characters");
        }
    }

    // Validator for Startup Founder creation (Updated)
    public class StartupFounderCreateDtoValidator : AbstractValidator<StartupFounderCreateDto>
    {
        public StartupFounderCreateDtoValidator()
        {
            // Removed Include(new UserCreateDtoValidator()); as fields differ significantly

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required")
                .Equal(x => x.Password).WithMessage("Passwords do not match");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required")
                .MaximumLength(100).WithMessage("Company name must not exceed 100 characters");

            // Removed rules for Bio and LinkedInProfile as they are not in the DTO
        }
    }

    // Validator for Employee creation, inheriting base user validation
    public class EmployeeCreateDtoValidator : AbstractValidator<EmployeeCreateDto>
    {
        public EmployeeCreateDtoValidator()
        {
            Include(new UserCreateDtoValidator()); // Assuming EmployeeCreateDto still inherits/matches UserCreateDto

            RuleFor(x => x.StartupId)
                .GreaterThan(0).WithMessage("Startup ID must be greater than 0");

            RuleFor(x => x.Position)
                .MaximumLength(100).WithMessage("Position must not exceed 100 characters");

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0).When(x => x.Salary.HasValue).WithMessage("Salary must be non-negative");

            RuleFor(x => x.CommissionRate)
                .InclusiveBetween(0, 1).When(x => x.CommissionRate.HasValue).WithMessage("Commission rate must be between 0 and 1");
        }
    }
}

