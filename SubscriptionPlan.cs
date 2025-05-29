using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDMS.Domain.Entities
{
    public class SubscriptionPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public int? MemberLimit { get; set; } // Nullable for unlimited

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerMember { get; set; }

        [StringLength(50)]
        public string? PriceDescription { get; set; } // e.g., "Custom", "Free"

        // Storing features as JSON string
        [Column(TypeName = "nvarchar(max)")]
        public string Features { get; set; }

        // Add any other plan-specific properties if needed
        // public string PlanType { get; set; } // Example: If needed directly on the entity
        // public decimal MonthlyCost { get; set; } // Example: If needed directly on the entity
    }
}

