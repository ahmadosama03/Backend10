using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// using SDMS.Domain.Entities.Startups; // Removed because the namespace does not exist or is unnecessary

namespace SDMS.Domain.Entities
{
    public class Report
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        public int GeneratedById { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ReportType { get; set; } // Financial, Employee, Startup, etc.
        
        [Required]
        [StringLength(10)]
        public string Format { get; set; } // PDF, Excel, Text
        
        [Required]
        [StringLength(255)]
        public string StoragePath { get; set; }
        
        public int? StartupId { get; set; } // NULL for system-wide reports
        
        public bool IsArchived { get; set; } = false;
        
        // Navigation properties
        public virtual User GeneratedBy { get; set; }
        public virtual Startup Startup { get; set; }
    }
}
