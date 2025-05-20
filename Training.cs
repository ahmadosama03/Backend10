using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDMS.Domain.Entities
{
    public class Training
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int EmployeeId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string TrainingName { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TrainingType { get; set; }
        
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? EndDate { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Cancelled
        
        public float CompletionPercentage { get; set; } = 0.0f;
        
        [StringLength(500)]
        public string Feedback { get; set; }
        
        // Navigation property
        public virtual Employee Employee { get; set; }
    }
}
