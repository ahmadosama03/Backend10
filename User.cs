using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SDMS.Domain.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        // Username might still be needed internally or for other login methods
        // If not strictly required by DB constraints, could be made nullable
        // For now, keep required and populate with Email during registration
        [Required]
        [StringLength(100)] // Increased length to accommodate emails
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        // Added FirstName and LastName as per frontend DTOs
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        // Made Name nullable as FirstName/LastName are primary now
        [StringLength(101)] // Max length for combined FirstName + LastName + space
        public string? Name { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; } // Correct type for varbinary mapping

        [Required]
        public byte[] PasswordSalt { get; set; } // Correct type for varbinary mapping

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; } // Kept nullable

        [Required]
        [StringLength(20)]
        public string Role { get; set; } // Admin, StartupFounder, Employee, Investor

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(255)]
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

        // Optional fields from frontend User type (can be added if needed for persistence)
        // public string? Avatar { get; set; }
        // public string? Bio { get; set; }
        // public int? Age { get; set; }
        // public bool? EmailPublic { get; set; }
        // public string? AccountType { get; set; } // Consider enum if used

        // Navigation properties
        public virtual Admin? Admin { get; set; }
        public virtual StartupFounder? StartupFounder { get; set; }
        public virtual Employee? Employee { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public virtual ICollection<Report> GeneratedReports { get; set; } = new List<Report>();
    }
}

