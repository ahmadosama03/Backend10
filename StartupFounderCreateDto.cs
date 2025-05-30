using System.ComponentModel.DataAnnotations;

namespace SDMS.Core.DTOs
{
    public class StartupFounderCreateDto
    {
        // Removed Username field

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } // Added for frontend validation

        [Required]
        [StringLength(50)] // Assuming max length for first name
        public string FirstName { get; set; } // Replaced Name

        [Required]
        [StringLength(50)] // Assuming max length for last name
        public string LastName { get; set; } // Replaced Name

        // Removed PhoneNumber field

        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; } // Specific to StartupFounder
    }
}

