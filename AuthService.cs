using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SDMS.Core.DTOs;
using SDMS.Domain.Entities;
using SDMS.Infrastructure.Data;
using AutoMapper;

namespace SDMS.Core.Services
{
    /// <summary>
    /// Service for handling authentication and user management in the SDMS system
    /// </summary>
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly AuditService _auditService;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IMapper mapper,
            AuditService auditService)
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;
            _auditService = auditService;
        }

        /// <summary>
        /// Authenticates a user and generates a JWT token
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>Authentication response with token or null if authentication fails</returns>
        public async Task<AuthResponseDto> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
                return null;

            if (!VerifyPasswordHash(
                password, 
                Convert.FromBase64String(user.PasswordHash), 
                Convert.FromBase64String(user.PasswordSalt)))
                return null;

            // Log successful login
            await _auditService.LogUserActionAsync(user.Id, "Login", "Successful login");

            // Generate token
            var token = GenerateJwtToken(user);
            
            // Get user role
            string role = "User";
            if (await _context.Admins.AnyAsync(a => a.User.Id == user.Id))
                role = "Admin";
            else if (await _context.StartupFounders.AnyAsync(f => f.User.Id == user.Id))
                role = "StartupFounder";
            else if (await _context.Employees.AnyAsync(e => e.User.Id == user.Id))
                role = "Employee";

            return new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = role,
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"]))
            };
        }

        /// <summary>
        /// Registers a new admin user
        /// </summary>
        /// <param name="registerDto">Admin registration data</param>
        /// <returns>Created admin user or null if registration fails</returns>
        public async Task<UserDto> RegisterAdminAsync(AdminCreateDto registerDto)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
                return null;

            // Create user with hashed password
            CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = _mapper.Map<User>(registerDto);
            user.PasswordHash = Convert.ToBase64String(passwordHash);
            user.PasswordSalt = Convert.ToBase64String(passwordSalt);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create admin profile
            var admin = new Admin
            {
                User = user,
                AdminLevel = registerDto.AdminLevel,
                Department = registerDto.Department
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            // Log user creation
            await _auditService.LogEntityChangeAsync("Create", user.Id, null, new { user.Id, user.Username, user.Email, Role = "Admin" });

            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Registers a new startup founder user
        /// </summary>
        /// <param name="registerDto">Startup founder registration data</param>
        /// <returns>Created startup founder user or null if registration fails</returns>
        public async Task<UserDto> RegisterStartupFounderAsync(StartupFounderCreateDto registerDto)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
                return null;

            // Create user with hashed password
            CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = _mapper.Map<User>(registerDto);
            user.PasswordHash = Convert.ToBase64String(passwordHash);
            user.PasswordSalt = Convert.ToBase64String(passwordSalt);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create startup founder profile
            var founder = new StartupFounder
            {
                User = user,
                CompanyName = registerDto.CompanyName
            };

            _context.StartupFounders.Add(founder);
            await _context.SaveChangesAsync();

            // Log user creation
            await _auditService.LogEntityChangeAsync("Create", user.Id, null, new { user.Id, user.Username, user.Email, Role = "StartupFounder" });

            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Registers a new employee user
        /// </summary>
        /// <param name="registerDto">Employee registration data</param>
        /// <returns>Created employee user or null if registration fails</returns>
        public async Task<UserDto> RegisterEmployeeAsync(EmployeeCreateDto registerDto)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
                return null;

            // Check if startup exists
            var startup = await _context.Startups.FindAsync(registerDto.StartupId);
            if (startup == null)
                return null;

            // Create user with hashed password
            CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = _mapper.Map<User>(registerDto);
            user.PasswordHash = Convert.ToBase64String(passwordHash);
            user.PasswordSalt = Convert.ToBase64String(passwordSalt);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create employee profile
            var employee = _mapper.Map<Employee>(registerDto);
            employee.UserId = user.Id;

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Log user creation
            await _auditService.LogEntityChangeAsync("Create", user.Id, null, new { user.Id, user.Username, user.Email, Role = "Employee" });

            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Changes a user's password
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="currentPassword">Current password</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if password change is successful, false otherwise</returns>
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            if (!VerifyPasswordHash(currentPassword, Convert.FromBase64String(user.PasswordHash), Convert.FromBase64String(user.PasswordSalt)))
                return false;

            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = Convert.ToBase64String(passwordHash);
            user.PasswordSalt = Convert.ToBase64String(passwordSalt);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Log password change
            await _auditService.LogUserActionAsync(userId, "PasswordChange", "Password changed successfully");

            return true;
        }

        /// <summary>
        /// Gets a user by ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User DTO</returns>
        public async Task<UserDto> GetUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Updates a user's profile
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="updateDto">User update data</param>
        /// <returns>True if update is successful, false otherwise</returns>
        public async Task<bool> UpdateUserAsync(int userId, UserUpdateDto updateDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            var oldUser = new
            {
                user.Email,
                user.PhoneNumber,
                user.IsActive
            };

            user.Email = updateDto.Email;
            user.PhoneNumber = updateDto.PhoneNumber;
            user.IsActive = updateDto.IsActive;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Log user update
            await _auditService.LogEntityChangeAsync("Update", userId, oldUser, new
            {
                updateDto.Email,
                updateDto.PhoneNumber,
                updateDto.IsActive
            });

            return true;
        }

        /// <summary>
        /// Deactivates a user account
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if deactivation is successful, false otherwise</returns>
        public async Task<bool> DeactivateUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.IsActive = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Log user deactivation
            await _auditService.LogUserActionAsync(userId, "Deactivate", "User account deactivated");

            return true;
        }

        /// <summary>
        /// Reactivates a user account
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if reactivation is successful, false otherwise</returns>
        public async Task<bool> ReactivateUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.IsActive = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Log user reactivation
            await _auditService.LogUserActionAsync(userId, "Reactivate", "User account reactivated");

            return true;
        }

        /// <summary>
        /// Generates a password reset token for a user
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>Password reset token or null if user not found</returns>
        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return null;

            // Generate a random token
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            
            // Store token with expiration (24 hours)
            user.ResetToken = token;
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(24);
            
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Log token generation
            await _auditService.LogUserActionAsync(user.Id, "PasswordResetRequested", "Password reset token generated");

            return token;
        }

        /// <summary>
        /// Resets a user's password using a reset token
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="token">Reset token</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if password reset is successful, false otherwise</returns>
        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.Email == email && 
                u.ResetToken == token && 
                u.ResetTokenExpires > DateTime.UtcNow);

            if (user == null)
                return false;

            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = Convert.ToBase64String(passwordHash);
            user.PasswordSalt = Convert.ToBase64String(passwordSalt);
            user.ResetToken = null;
            user.ResetTokenExpires = null;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Log password reset
            await _auditService.LogUserActionAsync(user.Id, "PasswordReset", "Password reset successfully");

            return true;
        }

        /// <summary>
        /// Creates a password hash
        /// </summary>
        /// <param name="password">Password</param>
        /// <param name="passwordHash">Output password hash</param>
        /// <param name="passwordSalt">Output password salt</param>
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        /// <summary>
        /// Verifies a password against a hash
        /// </summary>
        /// <param name="password">Password to verify</param>
        /// <param name="storedHash">Stored password hash</param>
        /// <param name="storedSalt">Stored password salt</param>
        /// <returns>True if password is correct, false otherwise</returns>
        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Generates a JWT token for a user
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>JWT token</returns>
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);
            
            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add role claim
            if (_context.Admins.Any(a => a.User.Id == user.Id))
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            else if (_context.StartupFounders.Any(f => f.User.Id == user.Id))
                claims.Add(new Claim(ClaimTypes.Role, "StartupFounder"));
            else if (_context.Employees.Any(e => e.User.Id == user.Id))
                claims.Add(new Claim(ClaimTypes.Role, "Employee"));
            else
                claims.Add(new Claim(ClaimTypes.Role, "User"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
