using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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

        public AuthController(AuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.AuthenticateAsync(loginDto.Username, loginDto.Password);
            if (result == null)
                return Unauthorized(new { message = "Invalid username or password" });

            return Ok(result);
        }

        [HttpPost("register/admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> RegisterAdmin(AdminCreateDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAdminAsync(registerDto);
            if (result == null)
                return BadRequest(new { message = "User registration failed" });

            return Ok(result);
        }

        [HttpPost("register/founder")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> RegisterStartupFounder(StartupFounderCreateDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterStartupFounderAsync(registerDto);
            if (result == null)
                return BadRequest(new { message = "User registration failed" });

            return Ok(result);
        }

        [HttpPost("register/employee")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<UserDto>> RegisterEmployee(EmployeeCreateDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterEmployeeAsync(registerDto);
            if (result == null)
                return BadRequest(new { message = "User registration failed" });

            return Ok(result);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(UserChangePasswordDto passwordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (passwordDto.NewPassword != passwordDto.ConfirmPassword)
                return BadRequest(new { message = "New password and confirmation password do not match" });

            var userId = int.Parse(User.FindFirst("UserId")?.Value);
            var result = await _authService.ChangePasswordAsync(userId, passwordDto.CurrentPassword, passwordDto.NewPassword);
            if (!result)
                return BadRequest(new { message = "Password change failed. Please check your current password." });

            return NoContent();
        }

        [HttpGet("validate")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            return Ok(new { isValid = true, userId = User.FindFirst("UserId")?.Value, role = User.FindFirst(ClaimTypes.Role)?.Value });
        }
    }
}
