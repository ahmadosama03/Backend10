using System.ComponentModel.DataAnnotations;

namespace SDMS.Core.DTOs
{
    public class UserLoginDto
    {
        [Required]
        [EmailAddress] // Changed from Username
        [StringLength(100)] // Adjusted length if needed
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)] // Assuming a minimum password length
        public string Password { get; set; }
    }
}

