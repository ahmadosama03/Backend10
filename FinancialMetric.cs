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
        
        public DateTime Date { get; set; } = DateTime.UtcNow;
        
        public float Revenue { get; set; } = 0.0f;
        
        public float Expenses { get; set; } = 0.0f;
        
        public float MonthlySales { get; set; } = 0.0f;
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        public bool IsArchived { get; set; } = false;
        
        // Navigation property
        public virtual Startup Startup { get; set; }
    }
}

// Define the Startup class if it does not exist
namespace SDMS.Domain.Entities
{
    public class Startup
    {
        public int Id { get; set; }
            public ICollection<FinancialMetric> FinancialMetrics { get; set; }
            public Subscription Subscription { get; set; }
            public ICollection<Report> Reports { get; set; }
            public ICollection<Employee> Employees { get; set; }

            public string SubscriptionStatus { get; set; }
           public DateTime FoundingDate { get; set; }
       public string Description { get; set; }
    public string Industry { get; set; }
    // other properties
 public int FounderId { get; set; }
    public StartupFounder Founder { get; set; }
        public string LogoUrl { get; set; }
        public string Name { get; set; }
    }
}
