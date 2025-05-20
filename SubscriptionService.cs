using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SDMS.Domain.Entities;
using SDMS.Core.DTOs;
using System.Linq;
using System.Text.Json;
using SDMS.Infrastructure.Data;

namespace SDMS.Core.Services
{
    public class SubscriptionService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SubscriptionDto> GetSubscriptionAsync(int startupId)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.StartupId == startupId);

            if (subscription == null)
                return null;

            return new SubscriptionDto
            {
                Id = subscription.Id,
                StartupId = subscription.StartupId,
                PlanType = subscription.PlanType,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                Cost = subscription.Cost,
                IsActive = subscription.IsActive,
                AutoRenew = subscription.AutoRenew,
                PaymentStatus = subscription.PaymentStatus,
                CostBreakdown = subscription.CostBreakdown != null 
                    ? JsonSerializer.Deserialize<Dictionary<string, decimal>>(subscription.CostBreakdown)
                    : new Dictionary<string, decimal>()
            };
        }

        public async Task<SubscriptionDto> CreateSubscriptionAsync(SubscriptionCreateDto dto)
        {
            // Check if startup already has a subscription
            var existingSubscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.StartupId == dto.StartupId);

            if (existingSubscription != null)
            {
                // Deactivate existing subscription
                existingSubscription.IsActive = false;
                await _context.SaveChangesAsync();
            }

            // Create cost breakdown
            var costBreakdown = new Dictionary<string, decimal>();
            
            switch (dto.PlanType)
            {
                case "Free":
                    costBreakdown.Add("Base", 0);
                    break;
                case "Pro":
                    costBreakdown.Add("Base", 50);
                    costBreakdown.Add("Support", 20);
                    costBreakdown.Add("Analytics", 30);
                    break;
                case "Growth":
                    costBreakdown.Add("Base", 100);
                    costBreakdown.Add("Support", 50);
                    costBreakdown.Add("Analytics", 50);
                    costBreakdown.Add("Advanced Features", 50);
                    break;
                case "Enterprise":
                    costBreakdown.Add("Base", 200);
                    costBreakdown.Add("Premium Support", 100);
                    costBreakdown.Add("Advanced Analytics", 100);
                    costBreakdown.Add("Custom Features", 100);
                    costBreakdown.Add("Dedicated Account Manager", 100);
                    break;
            }

            var subscription = new Subscription
            {
                StartupId = dto.StartupId,
                PlanType = dto.PlanType,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(dto.DurationMonths),
                Cost = dto.Cost,
                IsActive = true,
                AutoRenew = dto.AutoRenew,
                PaymentStatus = "Pending",
                CostBreakdown = JsonSerializer.Serialize(costBreakdown)
            };

            _context.Subscriptions.Add(subscription);
            
            // Update startup subscription status
            var startup = await _context.Startups.FindAsync(dto.StartupId);
            if (startup != null)
            {
                startup.SubscriptionStatus = dto.PlanType;
            }
            
            await _context.SaveChangesAsync();

            return new SubscriptionDto
            {
                Id = subscription.Id,
                StartupId = subscription.StartupId,
                PlanType = subscription.PlanType,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                Cost = subscription.Cost,
                IsActive = subscription.IsActive,
                AutoRenew = subscription.AutoRenew,
                PaymentStatus = subscription.PaymentStatus,
                CostBreakdown = costBreakdown
            };
        }

        public async Task<bool> UpdateSubscriptionAsync(int id, SubscriptionUpdateDto dto)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
                return false;

            subscription.AutoRenew = dto.AutoRenew;
            subscription.PaymentStatus = dto.PaymentStatus;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelSubscriptionAsync(int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
                return false;

            subscription.IsActive = false;
            subscription.AutoRenew = false;
            
            // Update startup subscription status to Free
            var startup = await _context.Startups.FindAsync(subscription.StartupId);
            if (startup != null)
            {
                startup.SubscriptionStatus = "Free";
            }
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RenewSubscriptionAsync(int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
                return false;

            // Create a new subscription period
            var newEndDate = subscription.EndDate.AddMonths(1);
            subscription.StartDate = subscription.EndDate;
            subscription.EndDate = newEndDate;
            subscription.PaymentStatus = "Pending";
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SubscriptionPlanDto>> GetAvailablePlansAsync()
        {
            // Return predefined subscription plans
            return new List<SubscriptionPlanDto>
            {
                new SubscriptionPlanDto
                {
                    PlanType = "Free",
                    MonthlyCost = 0,
                    Features = new List<string>
                    {
                        "Basic startup management",
                        "Up to 5 employees",
                        "Basic reporting",
                        "Email support"
                    }
                },
                new SubscriptionPlanDto
                {
                    PlanType = "Pro",
                    MonthlyCost = 100,
                    Features = new List<string>
                    {
                        "Advanced startup management",
                        "Up to 20 employees",
                        "Standard reporting",
                        "Email and chat support",
                        "Financial analytics"
                    }
                },
                new SubscriptionPlanDto
                {
                    PlanType = "Growth",
                    MonthlyCost = 250,
                    Features = new List<string>
                    {
                        "Comprehensive startup management",
                        "Up to 50 employees",
                        "Advanced reporting",
                        "Priority support",
                        "Advanced analytics",
                        "API access"
                    }
                },
                new SubscriptionPlanDto
                {
                    PlanType = "Enterprise",
                    MonthlyCost = 600,
                    Features = new List<string>
                    {
                        "Full-featured startup management",
                        "Unlimited employees",
                        "Custom reporting",
                        "24/7 dedicated support",
                        "Advanced analytics with predictions",
                        "API access",
                        "Custom integrations",
                        "Dedicated account manager"
                    }
                }
            };
        }
    }
}
