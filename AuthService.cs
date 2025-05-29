using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity; // Assuming Identity is used for user management
using Microsoft.EntityFrameworkCore; // If using EF Core
using Microsoft.Extensions.Configuration; // For JWT settings
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SDMS.Core.DTOs;
using SDMS.Domain.Entities;
using SDMS.Infrastructure.Data; // Assuming DbContext is here

namespace SDMS.Core.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context; // Replace with actual DbContext
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration; // For JWT token generation
        // Assuming UserManager and SignInManager if using ASP.NET Core Identity
        // private readonly UserManager<User> _userManager;
        // private readonly SignInManager<User> _signInManager;

        // Constructor without Identity
        public AuthService(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        // Placeholder for AuthenticateAsync - needs real implementation
        public async Task<AuthResponseDto> AuthenticateAsync(string username, string password)
        {
            // --- THIS IS A PLACEHOLDER --- 
            // Replace with actual authentication logic (e.g., check password hash)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username); // Or email

            if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) // Implement VerifyPasswordHash
            {
                return null; // Invalid credentials
            }

            // Generate JWT Token
            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = _mapper.Map<UserDto>(user)
            };
        }

        // Placeholder for RegisterAdminAsync - needs real implementation
        public async Task<UserDto> RegisterAdminAsync(AdminCreateDto registerDto)
        {
            // --- THIS IS A PLACEHOLDER --- 
            // Check if user exists, hash password, save to DB
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email))
            {
                return null; // User already exists
            }

            CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt); // Implement CreatePasswordHash

            var user = _mapper.Map<User>(registerDto);
            user.Role = "Admin"; // Set role
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }
        
        // Placeholder for RegisterStartupFounderAsync
        public async Task<UserDto> RegisterStartupFounderAsync(StartupFounderCreateDto registerDto)
        {
             // --- THIS IS A PLACEHOLDER --- 
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email))
            {
                return null; // User already exists
            }

            CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = _mapper.Map<User>(registerDto);
            user.Role = "StartupFounder";
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.CreatedAt = DateTime.UtcNow;
            // Link to Startup if needed, based on DTO

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        // Placeholder for RegisterEmployeeAsync
        public async Task<UserDto> RegisterEmployeeAsync(EmployeeCreateDto registerDto)
        {
             // --- THIS IS A PLACEHOLDER --- 
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email))
            {
                return null; // User already exists
            }

            CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = _mapper.Map<User>(registerDto);
            user.Role = "Employee"; // Or determine role from DTO
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.CreatedAt = DateTime.UtcNow;
            // Link to Employee entity / Startup if needed

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create Employee record (might be better in EmployeeService)
            var employee = new Employee
            {
                UserId = user.Id,
                StartupId = registerDto.StartupId, // Assuming StartupId is in DTO
                // Map other employee fields from DTO
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        // Placeholder for ChangePasswordAsync
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            // --- THIS IS A PLACEHOLDER --- 
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !VerifyPasswordHash(currentPassword, user.PasswordHash, user.PasswordSalt))
            {
                return false; // User not found or current password incorrect
            }

            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // --- NEW --- Placeholder for UpdateProfileAsync
        public async Task<UserDto> UpdateProfileAsync(int userId, UserProfileUpdateDto profileDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return null; // User not found
            }

            // Update allowed fields (e.g., Name, Email - check for uniqueness if needed)
            // Avoid updating Username, Role, Password here unless specifically intended
            user.Name = profileDto.Name ?? user.Name;
            
            // Handle email change carefully - might require verification
            if (!string.IsNullOrWhiteSpace(profileDto.Email) && !user.Email.Equals(profileDto.Email, StringComparison.OrdinalIgnoreCase))
            {
                 if (await _context.Users.AnyAsync(u => u.Email == profileDto.Email && u.Id != userId))
                 {
                     throw new ArgumentException("Email already in use."); // Or return a specific error response
                 }
                 user.Email = profileDto.Email;
                 // Consider adding email verification logic here
            }
            
            // Map other updatable profile fields from DTO
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        // --- Helper Methods (Implement these) ---
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(storedHash);
            }
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Username), // Often used for username
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name ?? ""), // User's full name
                new Claim(ClaimTypes.Role, user.Role)
                // Add other claims as needed
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:DurationInMinutes"])), // Use UtcNow
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

