using System;
using Microsoft.EntityFrameworkCore;
using SDMS.Domain.Entities;
//using SDMS.API;

namespace SDMS.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

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
                .OnDelete(DeleteBehavior.Restrict);

            // Startup configuration
            modelBuilder.Entity<Startup>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<Startup>()
                .HasOne(s => s.Founder)
                .WithMany(sf => sf.Startups)
                .HasForeignKey(s => s.FounderId)
                .OnDelete(DeleteBehavior.Restrict);

            // FinancialMetric configuration
            modelBuilder.Entity<FinancialMetric>()
                .HasOne(fm => fm.Startup)
                .WithMany(s => s.FinancialMetrics)
                .HasForeignKey(fm => fm.StartupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Subscription configuration
            modelBuilder.Entity<Subscription>()
                .HasOne(sub => sub.Startup)
                .WithOne(s => s.Subscription)
                .HasForeignKey<Subscription>(sub => sub.StartupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Training configuration
            modelBuilder.Entity<Training>()
                .HasOne(t => t.Employee)
                .WithMany()
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
                .OnDelete(DeleteBehavior.SetNull);

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
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
