using AutoMapper;
using SDMS.Domain.Entities;
using SDMS.Core.DTOs;
using System;

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

            // Admin mappings
            CreateMap<Admin, AdminDto>()
                .IncludeBase<User, UserDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.User.CreatedAt))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User.IsActive));
            
            CreateMap<AdminCreateDto, Admin>();

            // StartupFounder mappings
            CreateMap<StartupFounder, StartupFounderDto>()
                .IncludeBase<User, UserDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.User.CreatedAt))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User.IsActive));
            
            CreateMap<StartupFounderCreateDto, StartupFounder>();

            // Employee mappings
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber));
            
            CreateMap<EmployeeCreateDto, Employee>()
                .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.PerformanceScore, opt => opt.MapFrom(src => 0.0f));

            // Startup mappings
            CreateMap<Startup, StartupDto>()
                .ForMember(dest => dest.FounderName, opt => opt.MapFrom(src => 
                    src.Founder != null ? src.Founder.User.Username : "Unknown"));
            
            CreateMap<StartupCreateDto, Startup>()
                .ForMember(dest => dest.FoundingDate, opt => opt.MapFrom(src => 
                    src.FoundingDate.HasValue ? src.FoundingDate.Value : DateTime.UtcNow))
                .ForMember(dest => dest.SubscriptionStatus, opt => opt.MapFrom(src => "Free"));

            // FinancialMetric mappings
            CreateMap<FinancialMetric, FinancialMetricDto>()
                .ForMember(dest => dest.Profit, opt => opt.MapFrom(src => src.Revenue - src.Expenses));
            
            CreateMap<FinancialMetricCreateDto, FinancialMetric>()
                .ForMember(dest => dest.IsArchived, opt => opt.MapFrom(src => false));

            // Subscription mappings
            CreateMap<Subscription, SubscriptionDto>();
            CreateMap<SubscriptionCreateDto, Subscription>();

            // Training mappings
            CreateMap<Training, TrainingDto>();
            CreateMap<TrainingCreateDto, Training>();

            // Notification mappings
            CreateMap<Notification, NotificationDto>();
            CreateMap<NotificationCreateDto, Notification>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.DeliveryStatus, opt => opt.MapFrom(src => "Pending"));

            // AuditLog mappings
            CreateMap<AuditLog, AuditLogDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                    src.User != null ? src.User.Username : "System"));

            // Report mappings
            CreateMap<Report, ReportDto>()
                .ForMember(dest => dest.GeneratedBy, opt => opt.MapFrom(src => 
                    src.GeneratedBy != null ? src.GeneratedBy.Username : "System"));
        }
    }
}
