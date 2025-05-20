using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDMS.Domain.Entities
{
    public class StartupFounder
    {
        [Key]
        [ForeignKey("User")]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<Startup> Startups { get; set; }
    }
}
