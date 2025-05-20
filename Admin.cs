using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDMS.Domain.Entities
{
    public class Admin
    {
        [Key]
        [ForeignKey("User")]
        public int Id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string AdminLevel { get; set; } // SuperAdmin, SystemAdmin, etc.
        
        [StringLength(50)]
        public string Department { get; set; }
        
        // Navigation property
        public virtual User User { get; set; }
    }
}
