using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SDMS.Domain.Entities
{
    public class User
    {
        // ... other properties ...
public string ResetToken { get; set; }
public DateTime? ResetTokenExpires { get; set; }
// ... other properties ...
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
        public string PasswordHash { get; set; }
        
        [Required]
        public string PasswordSalt { get; set; }
        
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Role { get; set; } // Admin, StartupFounder, Employee, Investor
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual Admin Admin { get; set; }
        public virtual StartupFounder StartupFounder { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<AuditLog> AuditLogs { get; set; }
        public virtual ICollection<Report> GeneratedReports { get; set; }
    }
}
