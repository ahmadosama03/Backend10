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
        public int PlanId { get; set; } // Foreign key to SubscriptionPlan
        
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        
        public DateTime EndDate { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PricePaid { get; set; } // Price paid for this specific subscription period
        
        public bool IsActive { get; set; } = true;
        
        // Removed fields that might belong to Plan or are handled differently:
        // PlanType (derived from Plan)
        // Cost (derived from Plan, PricePaid stores actual amount)
        // AutoRenew (might be a setting elsewhere or on Startup)
        // PaymentStatus (might be tracked separately or implicitly by IsActive/EndDate)
        // CostBreakdown (could be stored on Plan or calculated)
        
        // Navigation properties
        [ForeignKey("StartupId")]
        public virtual Startup Startup { get; set; }
        
        [ForeignKey("PlanId")]
        public virtual SubscriptionPlan Plan { get; set; } // Navigation to the plan details
    }
}

