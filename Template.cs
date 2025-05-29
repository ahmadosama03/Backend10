using System;

namespace SDMS.Domain.Entities
{
    public class Template
    {
        public int Id { get; set; } // Using int to match other entities, frontend uses string ID but can be mapped.
        public string TemplateIdentifier { get; set; } // Corresponds to frontend id like 'physical', 'digital'
        public string Name { get; set; }
        public string Description { get; set; }

        // Add any other relevant properties for templates
    }
}

