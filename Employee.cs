using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDMS.Domain.Entities
{
    public class Employee
    {
        [Key]
        [ForeignKey("User")]
        public int Id { get; set; } // Links to User Id

        public int UserId { get; set; } // Explicit FK to User

        [Required]
        public int StartupId { get; set; }

        [StringLength(100)] // Added Position property
        public string? Position { get; set; }

        [Column(TypeName = "decimal(18,2)")] // Added Salary property
        public decimal? Salary { get; set; }

        [Column(TypeName = "decimal(5,4)")] // Added CommissionRate property (e.g., 0.1000 for 10%)
        public decimal? CommissionRate { get; set; }

        // Made HireDate nullable to match DTO and usage
        public DateTime? HireDate { get; set; } 

        [Required]
        [StringLength(50)]
        public string EmployeeRole { get; set; }

        public float PerformanceScore { get; set; } = 0.0f;

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Startup Startup { get; set; }
    }
}

