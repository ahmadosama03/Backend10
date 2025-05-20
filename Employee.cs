using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// using SDMS.Domain.Entities.Startups; // Removed: 'Startups' namespace does not exist

namespace SDMS.Domain.Entities
{
    public class Employee
    {
        [Key]
        [ForeignKey("User")]
        public int Id { get; set; }
    public int UserId { get; set; }
        [Required]
        public int StartupId { get; set; }

        [Required]
        [StringLength(50)]
        public string EmployeeRole { get; set; }

        public float PerformanceScore { get; set; } = 0.0f;

        public DateTime HireDate { get; set; } = DateTime.UtcNow;

        public virtual User User { get; set; }
                public Startup Startup { get; set; }

    }
}
// ... other using directives

