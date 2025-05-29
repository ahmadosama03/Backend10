using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SDMS.Domain.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(100)] // Added Name property based on build errors
        public string Name { get; set; } 

        [Required]
        public byte[] PasswordHash { get; set; } // Changed from string to byte[]

        [Required]
        public byte[] PasswordSalt { get; set; } // Changed from string to byte[]

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; } // Made nullable

        [Required]
        [StringLength(20)]
        public string Role { get; set; } // Admin, StartupFounder, Employee, Investor

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } // Added UpdatedAt property based on build errors

        public bool IsActive { get; set; } = true;

        [StringLength(255)] // Added ResetToken properties based on previous file content
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

        // Navigation properties
        public virtual Admin? Admin { get; set; }
        public virtual StartupFounder? StartupFounder { get; set; }
        public virtual Employee? Employee { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public virtual ICollection<Report> GeneratedReports { get; set; } = new List<Report>();
    }
}

