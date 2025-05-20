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
    public class SubscriptionsController : ControllerBase
    {
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionsController(SubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet("startup/{startupId}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<SubscriptionDto>> GetSubscription(int startupId)
        {
            var subscription = await _subscriptionService.GetSubscriptionAsync(startupId);
            if (subscription == null)
                return NotFound();

            return Ok(subscription);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<SubscriptionDto>> CreateSubscription(SubscriptionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subscription = await _subscriptionService.CreateSubscriptionAsync(dto);
            return CreatedAtAction(nameof(GetSubscription), new { startupId = subscription.StartupId }, subscription);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<IActionResult> UpdateSubscription(int id, SubscriptionUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _subscriptionService.UpdateSubscriptionAsync(id, dto);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<IActionResult> CancelSubscription(int id)
        {
            var result = await _subscriptionService.CancelSubscriptionAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{id}/renew")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<IActionResult> RenewSubscription(int id)
        {
            var result = await _subscriptionService.RenewSubscriptionAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("plans")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetAvailablePlans()
        {
            var plans = await _subscriptionService.GetAvailablePlansAsync();
            return Ok(plans);
        }
    }
}
