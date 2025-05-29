using System;
using System.Collections.Generic;

namespace SDMS.Domain.Entities
{
    public class Plan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? MemberLimit { get; set; } // Nullable for unlimited
        public decimal PricePerMember { get; set; } // Using decimal for currency
        public string PriceDescription { get; set; } // For custom pricing like "Custom"
        public string Features { get; set; } // Store features as JSON string or delimited string

        // Navigation property if needed (e.g., Startups using this plan)
        // public virtual ICollection<Startup> Startups { get; set; }
        // public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}

