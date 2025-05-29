using AutoMapper;
using SDMS.Domain.Entities;
using SDMS.Core.DTOs;
using System;
using System.Text.Json; // For deserializing features
using System.Collections.Generic; // For list features

namespace SDMS.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => 
                    src.Admin != null ? "Admin" : 
                    src.StartupFounder != null ? "StartupFounder" : 
                    src.Employee != null ? "Employee" : "Unknown"));
            
            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UserUpdateDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Ignore nulls during update

            // Admin mappings
            CreateMap<Admin, AdminDto>()
                .IncludeBase<User, UserDto>() // Include base user properties
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.User.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.User.UpdatedAt))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User.IsActive));
            
            CreateMap<AdminCreateDto, Admin>();

            // StartupFounder mappings
            CreateMap<StartupFounder, StartupFounderDto>()
                .IncludeBase<User, UserDto>() // Include base user properties
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.User.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.User.UpdatedAt))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User.IsActive));
            
            CreateMap<StartupFounderCreateDto, StartupFounder>();

            // Employee mappings
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber));
            
            CreateMap<EmployeeCreateDto, Employee>();
            CreateMap<EmployeeUpdateDto, Employee>()
                 .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Ignore nulls during update

            // Startup mappings
            CreateMap<Startup, StartupDto>()
                .ForMember(dest => dest.FounderName, opt => opt.MapFrom(src => src.Founder != null ? src.Founder.Name : "Unknown")) // Map FounderName from User.Name
                .ForMember(dest => dest.SubscriptionStatus, opt => opt.MapFrom(src => src.Subscription != null ? (src.Subscription.IsActive ? src.Subscription.Plan.Name : "Inactive") : "None")); // Map SubscriptionStatus
            
            CreateMap<StartupCreateDto, Startup>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow)); // Set CreatedAt on creation
                // FoundedDate is mapped directly if names match

            CreateMap<StartupUpdateDto, Startup>()
                 .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Ignore nulls during update

            // FinancialMetric mappings
            CreateMap<FinancialMetric, FinancialMetricDto>(); // Direct mapping is sufficient now
            CreateMap<FinancialMetricCreateDto, FinancialMetric>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date ?? DateTime.UtcNow)); // Handle nullable Date
            CreateMap<FinancialMetricUpdateDto, FinancialMetric>()
                 .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Ignore nulls during update

            // Subscription mappings
            CreateMap<Subscription, SubscriptionDto>()
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.Plan != null ? src.Plan.Name : "Unknown"));
            CreateMap<SubscriptionCreateDto, Subscription>();
            CreateMap<SubscriptionUpdateDto, Subscription>()
                 .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Ignore nulls during update

            // SubscriptionPlan mappings
            CreateMap<SubscriptionPlan, SubscriptionPlanDto>()
                .ForMember(dest => dest.Features, opt => opt.MapFrom(src => 
                    !string.IsNullOrEmpty(src.Features) ? JsonSerializer.Deserialize<List<string>>(src.Features, (JsonSerializerOptions)null) : new List<string>()))
                // Map PlanType and MonthlyCost if they exist on SubscriptionPlan entity
                // .ForMember(dest => dest.PlanType, opt => opt.MapFrom(src => src.PlanType)) 
                // .ForMember(dest => dest.MonthlyCost, opt => opt.MapFrom(src => src.MonthlyCost))
                ;

            // Training mappings
            CreateMap<Training, TrainingDto>();
            CreateMap<TrainingCreateDto, Training>();
            CreateMap<TrainingUpdateDto, Training>()
                 .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Ignore nulls during update

            // Notification mappings
            CreateMap<Notification, NotificationDto>();
            CreateMap<NotificationCreateDto, Notification>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => false));
                // Removed DeliveryStatus mapping as it's not in the Create DTO

            // AuditLog mappings
            CreateMap<AuditLog, AuditLogDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : "System"));

            // Report mappings
            CreateMap<Report, ReportDto>()
                .ForMember(dest => dest.GeneratedByName, opt => opt.MapFrom(src => src.GeneratedBy != null ? src.GeneratedBy.Name : "System")); // Map GeneratedByName from User.Name
            CreateMap<ReportGenerateDto, Report>()
                .ForMember(dest => dest.GeneratedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Template mappings
            CreateMap<Template, TemplateDto>();
        }
    }
}

