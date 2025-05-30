using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity; // Assuming Identity is used for user management
using Microsoft.EntityFrameworkCore; // If using EF Core
using Microsoft.Extensions.Configuration; // For JWT settings
using Microsoft.Extensions.Logging; // Added for logging
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
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger; // Added logger

        // Updated Constructor with Logger
        public AuthService(ApplicationDbContext context, IMapper mapper, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger; // Initialize logger
        }

        public async Task<AuthResponseDto> AuthenticateAsync(string email, string password)
        {
            try
            {
                // Find user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                // Check if user exists and password is correct
                if (user == null || user.PasswordHash == null || user.PasswordSalt == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                {
                    _logger.LogWarning("Authentication failed for email {Email}: Invalid credentials.", email);
                    return null; // Invalid credentials
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Authentication failed for email {Email}: User account is inactive.", email);
                    return null; // Account inactive
                }

                // Generate JWT Token
                var tokenInfo = GenerateJwtToken(user);
                if (tokenInfo == default) // Check if token generation failed
                {
                    _logger.LogError("JWT token generation failed for user {UserId}.", user.Id);
                    return null;
                }

                _logger.LogInformation("User {UserId} authenticated successfully.", user.Id);
                return new AuthResponseDto
                {
                    Token = tokenInfo.Token,
                    User = _mapper.Map<UserDto>(user),
                    ExpiresAt = tokenInfo.ExpiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during authentication for email {Email}.", email);
                // Do not return the exception details to the caller, just log it.
                return null; // Indicate failure without exposing internal error details
            }
        }

        public async Task<AuthResponseDto> RegisterStartupFounderAsync(StartupFounderCreateDto registerDto)
        {
            try
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                {
                    _logger.LogWarning("Founder registration failed: Email {Email} already exists.", registerDto.Email);
                    return null; // Email already exists
                }

                // Create password hash and salt
                CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

                // Create the User entity
                var user = new User
                {
                    Username = registerDto.Email, // Use Email as Username initially
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Name = $"{registerDto.FirstName} {registerDto.LastName}",
                    Role = "StartupFounder",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true // New users are active by default
                };

                // Add user to context
                _context.Users.Add(user);
                
                // --- Handle CompanyName --- 
                // Assuming a Startup entity exists and needs to be created/linked
                // This requires a Startup entity definition and potentially a StartupService
                // For now, let's assume we create a basic Startup record if CompanyName is provided.
                Startup newStartup = null;
                if (!string.IsNullOrWhiteSpace(registerDto.CompanyName))
                {
                    newStartup = new Startup // Assuming Startup entity exists with these properties
                    {
                        Name = registerDto.CompanyName,                        FounderId = user.Id, // Corrected property name
                        CreatedAt = DateTime.UtcNow
                        // Add other relevant Startup fields if necessary
                    };
                    _context.Startups.Add(newStartup); // Add the new startup to the context
                }
                
                // Save changes to the database (creates User and potentially Startup)
                await _context.SaveChangesAsync();
                
                // If Startup was created and linked via FK, user.Id should be populated now.
                // If linking needs to happen after User save, update Startup here:
                // if (newStartup != null) { newStartup.FounderUserId = user.Id; await _context.SaveChangesAsync(); }

                // Generate JWT Token
                var tokenInfo = GenerateJwtToken(user);
                if (tokenInfo == default)
                {
                    _logger.LogError("JWT token generation failed after registering user {UserId}.", user.Id);
                    // Consider rolling back user creation or marking user as inactive if token is essential
                    return null;
                }

                _logger.LogInformation("Startup Founder {UserId} registered successfully for company {CompanyName}.", user.Id, registerDto.CompanyName ?? "N/A");
                
                // Map User to UserDto for the response
                var userDto = _mapper.Map<UserDto>(user);

                return new AuthResponseDto
                {
                    Token = tokenInfo.Token,
                    User = userDto,
                    ExpiresAt = tokenInfo.ExpiresAt
                };
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error during founder registration for email {Email}.", registerDto.Email);
                // Handle specific DB errors if possible (e.g., constraints)
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during founder registration for email {Email}.", registerDto.Email);
                return null;
            }
        }

        // --- Helper Methods ---
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (string.IsNullOrEmpty(password))
                return false; // Or throw ArgumentNullException
            if (storedHash == null || storedHash.Length == 0)
                return false; // Or throw ArgumentNullException
            if (storedSalt == null || storedSalt.Length == 0)
                return false; // Or throw ArgumentNullException
                
            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(storedHash);
            }
        }

        private (string Token, DateTime ExpiresAt) GenerateJwtToken(User user)
        {
            try
            {
                var jwtKey = _configuration["Jwt:Key"];
                var jwtIssuer = _configuration["Jwt:Issuer"];
                var jwtAudience = _configuration["Jwt:Audience"];
                var durationString = _configuration["Jwt:DurationInMinutes"];

                if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience) || string.IsNullOrEmpty(durationString))
                {
                    _logger.LogError("JWT configuration (Key, Issuer, Audience, DurationInMinutes) is missing or incomplete in appsettings.");
                    return default; // Return default tuple to indicate failure
                }

                if (!int.TryParse(durationString, out int durationInMinutes))
                {
                     _logger.LogError("Invalid JWT configuration: DurationInMinutes ({DurationString}) is not a valid integer.", durationString);
                     return default;
                }

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var expiry = DateTime.UtcNow.AddMinutes(durationInMinutes);

                var claims = new List<Claim>
                {
                    new Claim("UserId", user.Id.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Username ?? user.Email), // Fallback to Email if Username is null
                    new Claim(ClaimTypes.Email, user.Email ?? ""), // Ensure email is not null
                    new Claim(ClaimTypes.Role, user.Role ?? "") // Ensure role is not null
                };

                // Add names if they exist
                if (!string.IsNullOrWhiteSpace(user.FirstName))
                    claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
                if (!string.IsNullOrWhiteSpace(user.LastName))
                    claims.Add(new Claim(ClaimTypes.Surname, user.LastName));
                if (!string.IsNullOrWhiteSpace(user.Name))
                    claims.Add(new Claim(ClaimTypes.Name, user.Name));
                else if (!string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName))
                     claims.Add(new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()));


                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: expiry,
                    signingCredentials: credentials);

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return (tokenString, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during JWT token generation for user {UserId}.", user?.Id ?? 0);
                return default; // Return default tuple on error
            }
        }
        
        // --- Other methods (RegisterAdminAsync, RegisterEmployeeAsync, ChangePasswordAsync, UpdateProfileAsync, AuthenticateExternalAsync) remain largely the same ---
        // --- but would benefit from similar try-catch blocks and logging --- 

        // Placeholder for RegisterAdminAsync - needs real implementation + try/catch + logging
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

        // Placeholder for RegisterEmployeeAsync + try/catch + logging
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

        // Placeholder for ChangePasswordAsync + try/catch + logging
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

        // Placeholder for UpdateProfileAsync + try/catch + logging
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

        // Placeholder for External Authentication (Google/Apple) + try/catch + logging
        public async Task<AuthResponseDto> AuthenticateExternalAsync(string provider, string idToken)
        {
            // --- THIS IS A PLACEHOLDER --- 
            // Requires actual implementation with libraries like Google.Apis.Auth
            ExternalUserPayload validatedPayload = null; 
            try
            {
                // Simulate validation
                if (provider == "Google")
                {
                    validatedPayload = new ExternalUserPayload { Email = "user.from.google@example.com", Name = "Google User", ProviderUserId = "google_user_id_123" };
                }
                else if (provider == "Apple")
                {
                    validatedPayload = new ExternalUserPayload { Email = "user.from.apple@example.com", Name = "Apple User", ProviderUserId = "apple_user_id_456" };
                }
                else
                {
                    _logger.LogWarning("Unsupported external provider: {Provider}", provider);
                    return null;
                }

                if (validatedPayload == null)
                {
                    _logger.LogWarning("External token validation failed for provider {Provider}.", provider);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating external token for provider {Provider}", provider);
                return null;
            }

            // Find or create user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == validatedPayload.Email);
            if (user == null)
            {
                user = new User
                {
                    Username = validatedPayload.Email,
                    Email = validatedPayload.Email,
                    Name = validatedPayload.Name,
                    Role = "StartupFounder", // Default role
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = new byte[0], 
                    PasswordSalt = new byte[0] 
                };
                _context.Users.Add(user);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Error creating new external user {Email}", validatedPayload.Email);
                    return null;
                }
            }
            else
            {
                // Optionally update user info from provider if needed
                // user.Name = validatedPayload.Name; 
                // await _context.SaveChangesAsync();
            }

            // Generate JWT token
            var tokenInfo = GenerateJwtToken(user);
             if (tokenInfo == default)
            {
                _logger.LogError("JWT token generation failed for external user {UserId}.", user.Id);
                return null;
            }

            return new AuthResponseDto
            {
                Token = tokenInfo.Token,
                User = _mapper.Map<UserDto>(user),
                ExpiresAt = tokenInfo.ExpiresAt
            };
        }

        // Helper class for conceptual payload
        private class ExternalUserPayload
        {
            public string Email { get; set; }
            public string Name { get; set; }
            public string ProviderUserId { get; set; }
        }
        
    } // End AuthService
} // End namespace

