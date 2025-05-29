using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDMS.Domain.Entities
{
    public class Report
    {
        [Key]
        public int Id { get; set; }
        
        // Removed Title and Description as they are not in the DTOs being used for creation/retrieval in ReportService
        // If needed, they should be added back and mappings updated
        // [Required]
        // [StringLength(100)]
        // public string Title { get; set; }
        
        // [StringLength(500)]
        // public string Description { get; set; }
        
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        public int GeneratedById { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ReportType { get; set; } // Financial, Employee, Startup, etc.
        
        // Removed Format as it's not consistently used/mapped in ReportService
        // [Required]
        // [StringLength(10)]
        // public string Format { get; set; } // PDF, Excel, Text
        
        // Renamed StoragePath to FilePath to match usage in ReportService
        [Required]
        [StringLength(500)] // Increased length for potentially longer paths
        public string FilePath { get; set; }
        
        // Added Parameters property to store report generation parameters (e.g., date range)
        [Column(TypeName = "nvarchar(max)")] // Use nvarchar(max) for potentially long JSON
        public string? Parameters { get; set; } 

        public int? StartupId { get; set; } // NULL for system-wide reports
        
        // Removed IsArchived as it's not consistently used/mapped in ReportService
        // public bool IsArchived { get; set; } = false;
        
        // Navigation properties
        [ForeignKey("GeneratedById")]
        public virtual User GeneratedBy { get; set; }
        
        [ForeignKey("StartupId")]
        public virtual Startup Startup { get; set; }
    }
}

