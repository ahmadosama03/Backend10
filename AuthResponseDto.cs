using System;

namespace SDMS.Core.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public UserDto User { get; set; } // Assuming UserDto exists and is appropriate
        public string? RefreshToken { get; set; } // Added optional RefreshToken
        public DateTime ExpiresAt { get; set; } // Added ExpiresAt (use DateTime)
    }
}

