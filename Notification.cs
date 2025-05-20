using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDMS.Domain.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Message { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRead { get; set; } = false;
        
        [Required]
        [StringLength(20)]
        public string NotificationType { get; set; } // Email, SMS, In-App
        
        [Required]
        [StringLength(20)]
        public string DeliveryStatus { get; set; } = "Pending"; // Pending, Delivered, Failed
        
        // Navigation property
        public virtual User User { get; set; }
    }
}
