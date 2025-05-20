using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDMS.Domain.Entities
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int StartupId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string PlanType { get; set; } // Free, Pro, Growth, Enterprise
        
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        
        public DateTime EndDate { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Cost { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public bool AutoRenew { get; set; } = false;
        
        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed, Cancelled
        
        // JSON string with cost breakdown details
        public string CostBreakdown { get; set; }
        
        // Navigation property
        public virtual Startup Startup { get; set; }
    }
}
