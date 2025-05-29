using System;
using Microsoft.EntityFrameworkCore;
using SDMS.Domain.Entities;
using System.Text.Json; // For Plan features serialization
using System.Collections.Generic; // For Plan features list

namespace SDMS.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Existing DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<StartupFounder> StartupFounders { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Startup> Startups { get; set; }
        public DbSet<FinancialMetric> FinancialMetrics { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Training> Trainings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Report> Reports { get; set; }

        // --- NEW DbSets ---
        public DbSet<Template> Templates { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } // Renamed from Plans

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Admin configuration
            modelBuilder.Entity<Admin>()
                .HasOne(a => a.User)
                .WithOne(u => u.Admin)
                .HasForeignKey<Admin>(a => a.Id)
                .OnDelete(DeleteBehavior.Cascade);

            // StartupFounder configuration
            modelBuilder.Entity<StartupFounder>()
                .HasOne(sf => sf.User)
                .WithOne(u => u.StartupFounder)
                .HasForeignKey<StartupFounder>(sf => sf.Id)
                .OnDelete(DeleteBehavior.Cascade);

            // Employee configuration
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Startup)
                .WithMany(s => s.Employees)
                .HasForeignKey(e => e.StartupId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict deletion of startup if employees exist

            // Startup configuration
            modelBuilder.Entity<Startup>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<Startup>()
                .HasOne(s => s.Founder) // Assuming User entity has StartupFounder navigation property
                .WithMany() // Assuming a User (Founder) can have multiple startups, but Startup has one FounderId
                .HasForeignKey(s => s.FounderId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent founder deletion if they own startups

            // FinancialMetric configuration
            modelBuilder.Entity<FinancialMetric>()
                .Property(fm => fm.Value)
                .HasColumnType("decimal(18,2)"); // Define precision for financial values

            modelBuilder.Entity<FinancialMetric>()
                .HasOne(fm => fm.Startup)
                .WithMany(s => s.FinancialMetrics)
                .HasForeignKey(fm => fm.StartupId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete metrics if startup is deleted

            // Subscription configuration
            modelBuilder.Entity<Subscription>()
                .HasOne(sub => sub.Startup)
                .WithOne(s => s.Subscription)
                .HasForeignKey<Subscription>(sub => sub.StartupId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- UPDATED --- Add relationship to SubscriptionPlan
            modelBuilder.Entity<Subscription>()
                .HasOne(sub => sub.Plan) // Navigation property defined in Subscription entity
                .WithMany() // Assuming a Plan can have multiple Subscriptions
                .HasForeignKey(sub => sub.PlanId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a Plan if Subscriptions exist

            // Training configuration
            modelBuilder.Entity<Training>()
                .HasOne(t => t.Employee)
                .WithMany() // Assuming Employee doesn't have a direct collection of Trainings
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification configuration
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // AuditLog configuration
            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.SetNull); // Keep audit log even if user is deleted

            // Report configuration
            modelBuilder.Entity<Report>()
                .HasOne(r => r.GeneratedBy)
                .WithMany(u => u.GeneratedReports)
                .HasForeignKey(r => r.GeneratedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Startup)
                .WithMany(s => s.Reports)
                .HasForeignKey(r => r.StartupId)
                .OnDelete(DeleteBehavior.SetNull); // Keep report even if startup is deleted

            // --- NEW --- Template configuration
            modelBuilder.Entity<Template>()
                .HasIndex(t => t.TemplateIdentifier)
                .IsUnique();
            modelBuilder.Entity<Template>()
                .Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);
            modelBuilder.Entity<Template>()
                .Property(t => t.Description)
                .HasMaxLength(500);

            // --- NEW --- SubscriptionPlan configuration (Renamed from Plan)
            modelBuilder.Entity<SubscriptionPlan>()
                .HasIndex(p => p.Name)
                .IsUnique();
            modelBuilder.Entity<SubscriptionPlan>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);
            modelBuilder.Entity<SubscriptionPlan>()
                .Property(p => p.PricePerMember)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SubscriptionPlan>()
                .Property(p => p.PriceDescription)
                .HasMaxLength(50); // For "Custom", "Free", etc.
            modelBuilder.Entity<SubscriptionPlan>()
                .Property(p => p.Features) // Storing features as JSON string
                .HasColumnType("nvarchar(max)"); // Use nvarchar(max) for potentially long JSON
                
            // Seed initial data for Templates and SubscriptionPlans if desired
            modelBuilder.Entity<Template>().HasData(
                new Template { Id = 1, TemplateIdentifier = "physical", Name = "Physical Product", Description = "For startups selling physical products, with inventory management and supply chain features." },
                new Template { Id = 2, TemplateIdentifier = "digital", Name = "Digital Product", Description = "For software, digital goods, or online services with subscription management." },
                new Template { Id = 3, TemplateIdentifier = "service", Name = "Service-Based", Description = "For consulting firms, agencies, and service providers with client management." },
                new Template { Id = 4, TemplateIdentifier = "hybrid", Name = "Hybrid", Description = "For businesses with both physical and digital components, maximum flexibility." }
            );

            // Seed SubscriptionPlan data
            modelBuilder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan { Id = 1, Name = "Free", MemberLimit = 1, PricePerMember = 0, PriceDescription = "0", Features = JsonSerializer.Serialize(new List<string>{ "Single startup", "Up to 1 team member", "Basic dashboard", "Starter templates", "Email support" }) },
                new SubscriptionPlan { Id = 2, Name = "Pro", MemberLimit = 25, PricePerMember = 2, PriceDescription = "2", Features = JsonSerializer.Serialize(new List<string>{ "Pay only for management members", "Multiple startups", "Advanced dashboard", "All templates", "Priority email support", "API access", "Detailed reports", "Team permissions" }) },
                new SubscriptionPlan { Id = 3, Name = "Growth", MemberLimit = 100, PricePerMember = 5, PriceDescription = "5", Features = JsonSerializer.Serialize(new List<string>{ "Pay only for management members", "Unlimited startups", "Premium dashboard", "All templates", "Priority support 24/7", "Advanced analytics", "Custom reports", "Full API access", "Advanced permissions", "All integrations" }) },
                new SubscriptionPlan { Id = 4, Name = "Enterprise", MemberLimit = null, PricePerMember = 0, PriceDescription = "Custom", Features = JsonSerializer.Serialize(new List<string>{ "Unlimited management members", "Enterprise dashboard", "Custom templates", "Dedicated account manager", "White-glove service", "Custom integrations", "On-premise option", "SSO authentication", "Advanced security", "Custom training" }) }
            );
        }
    }
}

