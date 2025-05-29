using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SDMS.Core.DTOs;
using SDMS.Domain.Entities; // Assuming SubscriptionPlan entity is here if using DB
using SDMS.Infrastructure.Data; // Assuming DbContext is here
using System.Text.Json; // For deserializing features

namespace SDMS.Core.Services
{
    public class SubscriptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        // Mock plans data - Refactored to use SubscriptionPlanDto
        // Keep this temporarily until DB is fully set up and seeded
        private static readonly List<SubscriptionPlanDto> _mockPlans = new List<SubscriptionPlanDto>
        {
            new SubscriptionPlanDto { Id = 1, Name = "Free", MemberLimit = 1, PricePerMember = 0, PriceDescription = "0", Features = new List<string>{ "Single startup", "Up to 1 team member", "Basic dashboard", "Starter templates", "Email support" }, PlanType = "Free", MonthlyCost = 0 },
            new SubscriptionPlanDto { Id = 2, Name = "Pro", MemberLimit = 25, PricePerMember = 2, PriceDescription = "2", Features = new List<string>{ "Pay only for management members", "Multiple startups", "Advanced dashboard", "All templates", "Priority email support", "API access", "Detailed reports", "Team permissions" }, PlanType = "Pro", MonthlyCost = 2 },
            new SubscriptionPlanDto { Id = 3, Name = "Growth", MemberLimit = 100, PricePerMember = 5, PriceDescription = "5", Features = new List<string>{ "Pay only for management members", "Unlimited startups", "Premium dashboard", "All templates", "Priority support 24/7", "Advanced analytics", "Custom reports", "Full API access", "Advanced permissions", "All integrations" }, PlanType = "Growth", MonthlyCost = 5 },
            new SubscriptionPlanDto { Id = 4, Name = "Enterprise", MemberLimit = null, PricePerMember = 0, PriceDescription = "Custom", Features = new List<string>{ "Unlimited management members", "Enterprise dashboard", "Custom templates", "Dedicated account manager", "White-glove service", "Custom integrations", "On-premise option", "SSO authentication", "Advanced security", "Custom training" }, PlanType = "Enterprise", MonthlyCost = 0 }, // Assuming custom cost means 0 here, adjust as needed
        };

        public SubscriptionService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<SubscriptionDto> GetSubscriptionAsync(int startupId)
        {
            var subscription = await _context.Subscriptions
                                             .Include(s => s.Plan) // Include Plan details
                                             .FirstOrDefaultAsync(s => s.StartupId == startupId);
            // Manual mapping if AutoMapper isn't configured for PlanName
            var subDto = _mapper.Map<SubscriptionDto>(subscription);
            if (subDto != null && subscription?.Plan != null)
            {
                subDto.PlanName = subscription.Plan.Name;
            }
            return subDto;
        }

        public async Task<SubscriptionDto> CreateSubscriptionAsync(SubscriptionCreateDto dto)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(dto.PlanId); // Fetch the plan from DB
            if (plan == null) throw new ArgumentException("Invalid PlanId");

            var subscription = _mapper.Map<Subscription>(dto);
            subscription.StartDate = DateTime.UtcNow;
            // Calculate EndDate based on plan (e.g., 1 month, 1 year) - Assuming 1 year for now
            subscription.EndDate = DateTime.UtcNow.AddYears(1); 
            subscription.IsActive = true;
            subscription.PricePaid = plan.PricePerMember; // Set price based on plan (adjust logic as needed)
            subscription.PlanId = plan.Id; // Ensure PlanId is set

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();
            
            // Map back to DTO, including PlanName
            var subDto = _mapper.Map<SubscriptionDto>(subscription);
            subDto.PlanName = plan.Name;
            return subDto;
        }

        public async Task<bool> UpdateSubscriptionAsync(int id, SubscriptionUpdateDto dto)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
                return false;

            _mapper.Map(dto, subscription);
            // Add logic if changing plan or other details is allowed

            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelSubscriptionAsync(int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
                return false;

            subscription.IsActive = false;
            // Decide whether to set EndDate to now or keep original
            // subscription.EndDate = DateTime.UtcNow; 

            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RenewSubscriptionAsync(int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null || !subscription.IsActive) 
                return false;

            // Extend EndDate based on plan duration - Assuming 1 year renewal
            subscription.EndDate = subscription.EndDate.AddYears(1); 

            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();
            return true;
        }

        // Updated to return SubscriptionPlanDto
        public async Task<IEnumerable<SubscriptionPlanDto>> GetAvailablePlansAsync()
        {
            // Simulate async operation - Replace with DB query later
            await Task.Delay(50);
            // var plans = await _context.SubscriptionPlans.ToListAsync(); // Use this when DB is ready
            // return _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
            
            // Using mock data for now
            // No mapping needed as _mockPlans is already List<SubscriptionPlanDto>
            return _mockPlans; 
        }
    }
}

