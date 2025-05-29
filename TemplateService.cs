using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SDMS.Core.DTOs;
using SDMS.Domain.Entities; // Assuming Template entity is here

namespace SDMS.Core.Services
{
    public class TemplateService
    {
        private readonly IMapper _mapper;

        // In a real application, this would likely come from a database context
        private static readonly List<Template> _mockTemplates = new List<Template>
        {
            new Template { Id = 1, TemplateIdentifier = "physical", Name = "Physical Product", Description = "For startups selling physical products, with inventory management and supply chain features." },
            new Template { Id = 2, TemplateIdentifier = "digital", Name = "Digital Product", Description = "For software, digital goods, or online services with subscription management." },
            new Template { Id = 3, TemplateIdentifier = "service", Name = "Service-Based", Description = "For consulting firms, agencies, and service providers with client management." },
            new Template { Id = 4, TemplateIdentifier = "hybrid", Name = "Hybrid", Description = "For businesses with both physical and digital components, maximum flexibility." },
        };

        public TemplateService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<IEnumerable<TemplateDto>> GetTemplatesAsync()
        {
            // Simulate async operation
            await Task.Delay(50); 
            return _mapper.Map<IEnumerable<TemplateDto>>(_mockTemplates);
        }

        public async Task<TemplateDto> GetTemplateByIdAsync(string templateIdentifier)
        {
            // Simulate async operation
            await Task.Delay(50);
            var template = _mockTemplates.FirstOrDefault(t => t.TemplateIdentifier.Equals(templateIdentifier, StringComparison.OrdinalIgnoreCase));
            return _mapper.Map<TemplateDto>(template);
        }

        // Add methods for Create, Update, Delete if needed later
    }
}

