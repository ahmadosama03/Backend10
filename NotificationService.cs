using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SDMS.Domain.Entities;
using SDMS.Core.DTOs;
using System.Linq;
using SDMS.Infrastructure.Data;

namespace SDMS.Core.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead,
                NotificationType = n.NotificationType,
                DeliveryStatus = n.DeliveryStatus
            });
        }

        public async Task<NotificationDto> GetNotificationAsync(int id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
                return null;

            return new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead,
                NotificationType = notification.NotificationType,
                DeliveryStatus = notification.DeliveryStatus
            };
        }

        public async Task<NotificationDto> CreateNotificationAsync(NotificationCreateDto dto)
        {
            var notification = new Notification
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Message = dto.Message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                NotificationType = dto.NotificationType,
                DeliveryStatus = "Pending"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // In a real application, you would send the notification via the appropriate channel here
            // For example, send an email, SMS, or push notification
            await DeliverNotificationAsync(notification);

            return new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead,
                NotificationType = notification.NotificationType,
                DeliveryStatus = notification.DeliveryStatus
            };
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteNotificationAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<IEnumerable<NotificationDto>> CreateBulkNotificationsAsync(BulkNotificationCreateDto dto)
        {
            var notifications = new List<Notification>();

            foreach (var userId in dto.UserIds)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = dto.Title,
                    Message = dto.Message,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    NotificationType = dto.NotificationType,
                    DeliveryStatus = "Pending"
                };

                notifications.Add(notification);
            }

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            // Deliver notifications asynchronously
            foreach (var notification in notifications)
            {
                await DeliverNotificationAsync(notification);
            }

            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead,
                NotificationType = n.NotificationType,
                DeliveryStatus = n.DeliveryStatus
            });
        }

        private async Task DeliverNotificationAsync(Notification notification)
        {
            // In a real application, this would send the notification via the appropriate channel
            // For example, send an email, SMS, or push notification
            // This is a simplified implementation for demonstration purposes

            // Simulate delivery delay
            await Task.Delay(100);

            // Update delivery status
            notification.DeliveryStatus = "Delivered";
            await _context.SaveChangesAsync();
        }
    }
}
