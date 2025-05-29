using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDMS.Domain.Entities
{
    public class Startup
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string? Industry { get; set; }

        [MaxLength(50)]
        public string? Stage { get; set; }

        public string? Description { get; set; }

        [MaxLength(255)]
        public string? Website { get; set; }

        [MaxLength(512)]
        public string? LogoUrl { get; set; }

        [Required]
        public int FounderId { get; set; }

        public DateTime? FoundedDate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } // Added UpdatedAt

        [MaxLength(50)]
        public string? SubscriptionStatus { get; set; } // Added from misplaced properties

        // Navigation Properties
        [ForeignKey("FounderId")]
        public virtual User Founder { get; set; } // Changed from StartupFounder to User based on context

        public virtual Subscription? Subscription { get; set; }
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<FinancialMetric> FinancialMetrics { get; set; } = new List<FinancialMetric>();
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}

