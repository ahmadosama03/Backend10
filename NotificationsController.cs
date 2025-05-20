using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SDMS.Core.Services;
using SDMS.Core.DTOs;

namespace SDMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationsController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUserNotifications(int userId)
        {
            // Check if the current user is requesting their own notifications or if they're an admin
            if (userId != int.Parse(User.FindFirst("UserId")?.Value) && !User.IsInRole("Admin"))
                return Forbid();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<NotificationDto>> GetNotification(int id)
        {
            var notification = await _notificationService.GetNotificationAsync(id);
            if (notification == null)
                return NotFound();

            // Check if the current user is requesting their own notification or if they're an admin
            if (notification.UserId != int.Parse(User.FindFirst("UserId")?.Value) && !User.IsInRole("Admin"))
                return Forbid();

            return Ok(notification);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<NotificationDto>> CreateNotification(NotificationCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var notification = await _notificationService.CreateNotificationAsync(dto);
            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> CreateBulkNotifications(BulkNotificationCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var notifications = await _notificationService.CreateBulkNotificationsAsync(dto);
            return Ok(notifications);
        }

        [HttpPost("{id}/read")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _notificationService.GetNotificationAsync(id);
            if (notification == null)
                return NotFound();

            // Check if the current user is marking their own notification as read
            if (notification.UserId != int.Parse(User.FindFirst("UserId")?.Value))
                return Forbid();

            var result = await _notificationService.MarkAsReadAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _notificationService.GetNotificationAsync(id);
            if (notification == null)
                return NotFound();

            // Check if the current user is deleting their own notification or if they're an admin
            if (notification.UserId != int.Parse(User.FindFirst("UserId")?.Value) && !User.IsInRole("Admin"))
                return Forbid();

            var result = await _notificationService.DeleteNotificationAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("unread-count/{userId}")]
        [Authorize]
        public async Task<ActionResult<int>> GetUnreadCount(int userId)
        {
            // Check if the current user is requesting their own unread count or if they're an admin
            if (userId != int.Parse(User.FindFirst("UserId")?.Value) && !User.IsInRole("Admin"))
                return Forbid();

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(count);
        }
    }
}
