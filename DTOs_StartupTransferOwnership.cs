using System;
using System.ComponentModel.DataAnnotations;

namespace SDMS.Core.DTOs
{
    // DTO for transferring startup ownership
    public class StartupTransferOwnershipDto
    {
        [Required]
        public int NewFounderUserId { get; set; }
    }
}

