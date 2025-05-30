namespace SDMS.Core.DTOs
{
    public class ExternalLoginDto
    {
        public string Provider { get; set; } // e.g., "Google", "Apple"
        public string IdToken { get; set; }
    }
}

