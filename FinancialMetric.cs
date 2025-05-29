using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDMS.Domain.Entities
{
    public class FinancialMetric
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StartupId { get; set; }

        [Required]
        [MaxLength(50)] // Added MaxLength
        public string MetricType { get; set; } // Added MetricType (e.g., "Revenue", "Expense", "CustomerCount")

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Use decimal for financial values
        public decimal Value { get; set; } // Consolidated value field

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow; // Date the metric applies to

        [MaxLength(20)] // Added MaxLength
        public string Period { get; set; } // Added Period (e.g., "Monthly", "Quarterly", "Annual")

        [StringLength(500)]
        public string? Notes { get; set; } // Made nullable

        public bool IsArchived { get; set; } = false;

        // Navigation property
        public virtual Startup Startup { get; set; }
    }
}

// Define the Startup class if it does not exist
// --- This duplicate definition should be removed, assuming Startup.cs exists --- 
// namespace SDMS.Domain.Entities
// {
//     public class Startup
//     {
//         public int Id { get; set; }
//             public ICollection<FinancialMetric> FinancialMetrics { get; set; }
//             public Subscription Subscription { get; set; }
//             public ICollection<Report> Reports { get; set; }
//             public ICollection<Employee> Employees { get; set; }

//             public string SubscriptionStatus { get; set; }
//            public DateTime FoundingDate { get; set; }
//        public string Description { get; set; }
//     public string Industry { get; set; }
//     // other properties
//  public int FounderId { get; set; }
//     public StartupFounder Founder { get; set; }
//         public string LogoUrl { get; set; }
//         public string Name { get; set; }
//     }
// }

