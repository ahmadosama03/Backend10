using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SDMS.Core.Services;
using SDMS.Core.DTOs;
using SDMS.Domain.Entities;

namespace SDMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _authService = authService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous] // Ensure login is accessible
        public async Task<ActionResult<AuthResponseDto>> Login(UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.AuthenticateAsync(loginDto.Email, loginDto.Password);
                if (result == null)
                    return Unauthorized(new { message = "Invalid email or password" }); // More specific message

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for email {Email}.", loginDto.Email);
                // Return a generic 500 error to avoid leaking details
                return StatusCode(500, new { message = "An internal server error occurred during login." });
            }
        }

        [HttpPost("register/admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> RegisterAdmin(AdminCreateDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            try
            {
                var result = await _authService.RegisterAdminAsync(registerDto);
                if (result == null)
                    return BadRequest(new { message = "Admin registration failed. User might already exist." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred during admin registration for email {Email}.", registerDto.Email);
                return StatusCode(500, new { message = "An internal server error occurred during admin registration." });
            }
        }

        [HttpPost("register/founder")]
        [AllowAnonymous] // Allow public access for founder registration
        public async Task<ActionResult<AuthResponseDto>> RegisterStartupFounder(StartupFounderCreateDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.RegisterStartupFounderAsync(registerDto);
                if (result == null)
                    // Consider more specific error based on AuthService logic (e.g., email exists)
                    return BadRequest(new { message = "Founder registration failed. Email might already be in use." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during founder registration for email {Email}.", registerDto.Email);
                return StatusCode(500, new { message = "An internal server error occurred during founder registration." });
            }
        }

        [HttpPost("register/employee")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<UserDto>> RegisterEmployee(EmployeeCreateDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.RegisterEmployeeAsync(registerDto);
                if (result == null)
                    return BadRequest(new { message = "Employee registration failed. User might already exist." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during employee registration for email {Email}.", registerDto.Email);
                return StatusCode(500, new { message = "An internal server error occurred during employee registration." });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(UserChangePasswordDto passwordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ConfirmPassword check should be in validator, but double-check here is fine
            if (passwordDto.NewPassword != passwordDto.ConfirmPassword)
                return BadRequest(new { message = "New password and confirmation password do not match" });

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }
            
            try
            {
                var result = await _authService.ChangePasswordAsync(userId, passwordDto.CurrentPassword, passwordDto.NewPassword);
                if (!result)
                    return BadRequest(new { message = "Password change failed. Please check your current password." });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during password change for user {UserId}.", userId);
                return StatusCode(500, new { message = "An internal server error occurred during password change." });
            }
        }

        [HttpGet("validate")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            // This endpoint confirms the token is valid by reaching here due to [Authorize]
            return Ok(new { isValid = true, userId = User.FindFirst("UserId")?.Value, role = User.FindFirst(ClaimTypes.Role)?.Value });
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<UserDto>> UpdateProfile(UserProfileUpdateDto profileDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            try
            {
                var updatedUser = await _authService.UpdateProfileAsync(userId, profileDto);
                if (updatedUser == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                return Ok(updatedUser);
            }
            catch (ArgumentException ex) // Catch specific exceptions like email conflict
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the profile for user {UserId}.", userId);
                return StatusCode(500, new { message = "An error occurred while updating the profile." });
            }
        }

        // --- NEW External Login Endpoints ---
        [HttpPost("login/google")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] ExternalLoginDto loginDto)
        {
            if (!ModelState.IsValid || loginDto.Provider != "Google")
            {
                return BadRequest(new { message = "Invalid request for Google login." });
            }

            try
            {
                var result = await _authService.AuthenticateExternalAsync(loginDto.Provider, loginDto.IdToken);
                if (result == null)
                {
                    return Unauthorized(new { message = "Google authentication failed or user not found/created." });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during Google login.");
                return StatusCode(500, new { message = "An internal error occurred during Google login." });
            }
        }

        [HttpPost("login/apple")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> AppleLogin([FromBody] ExternalLoginDto loginDto)
        {
            if (!ModelState.IsValid || loginDto.Provider != "Apple")
            {
                return BadRequest(new { message = "Invalid request for Apple login." });
            }

            try
            {
                var result = await _authService.AuthenticateExternalAsync(loginDto.Provider, loginDto.IdToken);
                if (result == null)
                {
                    return Unauthorized(new { message = "Apple authentication failed or user not found/created." });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during Apple login.");
                return StatusCode(500, new { message = "An internal error occurred during Apple login." });
            }
        }

        // --- NEW Logout Endpoint ---
        [HttpPost("logout")]
        [Authorize] // Require authentication to logout
        public IActionResult Logout()
        {
            // Server-side logout logic (if any) would go here.
            // For JWT, logout is typically handled client-side by discarding the token.
            // This endpoint can be used to acknowledge the logout request or perform server-side cleanup if needed.
            _logger.LogInformation("User {UserId} logged out.", User.FindFirst("UserId")?.Value ?? "Unknown");
            return Ok(new { message = "Logout successful" }); // Or NoContent();
        }

    } // End of AuthController class
} // End of namespace

