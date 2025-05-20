using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDMS.Domain.Entities
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        
        public int? UserId { get; set; } // NULL for system actions
        
        [Required]
        [StringLength(50)]
        public string Action { get; set; }
        
        [Required]
        [StringLength(50)]
        public string EntityName { get; set; }
        
        public int? EntityId { get; set; }
        
        public string OldValues { get; set; }
        
        public string NewValues { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [StringLength(50)]
        public string IpAddress { get; set; }
        
        // Navigation property
        public virtual User User { get; set; }
    }
}
