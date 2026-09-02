using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using UserService.Models;
using Microsoft.AspNetCore.Mvc;
using UserService.Data;
using UserService.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PasswordService;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly appDataContext _context;
        private readonly IConfiguration _config;

        public UserController(appDataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return Conflict("Email already exists.");

            var hashPassword = PasswordHasher.Hash(dto.Password);
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = hashPassword,
                Role = string.IsNullOrEmpty(dto.Role) ? "User" : dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized("Invalid credentials.");

            if (!PasswordHasher.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid Password.");

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateSecureToken();

            // Store refresh token
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.ID,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return Ok(new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
        }

        // GET api/User/me - Get current logged-in user (must be before {id} route)
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();
            return Ok(new { user.ID, user.FirstName, user.LastName, user.Email, user.Role, user.EmailVerified });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(new { user.ID, user.FirstName, user.LastName, user.Email, user.Role });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var totalCount = await _context.Users.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var users = await _context.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new { u.ID, u.FirstName, u.LastName, u.Email, u.Role })
                .ToListAsync();

            return Ok(new
            {
                data = users,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages
                }
            });
        }

        // PUT api/User/{id} - Update user details
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var userId = GetUserId();
            if (userId != id && !User.IsInRole("Admin"))
                return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Check if new email already exists
            if (user.Email != dto.Email && await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return Conflict("Email already exists.");

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;

            await _context.SaveChangesAsync();
            return Ok(new { user.ID, user.FirstName, user.LastName, user.Email, user.Role });
        }

        // DELETE api/User/{id} - Delete user account
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userId = GetUserId();
            if (userId != id && !User.IsInRole("Admin"))
                return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST api/User/change-password - Change password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (!PasswordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
                return BadRequest("Current password is incorrect.");

            user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Password changed successfully." });
        }

        // POST api/User/forgot-password - Request password reset
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Ok(new { message = "If email exists, reset link will be sent." }); // Security: Don't reveal if email exists

            // Generate reset token
            var token = GenerateSecureToken();
            var resetToken = new PasswordResetToken
            {
                UserId = user.ID,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            // TODO: Send email with reset link containing token
            return Ok(new { message = "If email exists, reset link will be sent." });
        }

        // POST api/User/reset-password - Reset password with token
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return BadRequest("Invalid request.");

            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == dto.Token && t.UserId == user.ID && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

            if (resetToken == null)
                return BadRequest("Invalid or expired token.");

            user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
            resetToken.IsUsed = true;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Password reset successfully." });
        }

        // POST api/User/refresh-token - Refresh JWT token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);

            if (refreshToken == null)
                return Unauthorized("Invalid or expired refresh token.");

            var user = refreshToken.User;
            if (user == null) return Unauthorized();

            // Generate new tokens
            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateSecureToken();

            // Revoke old refresh token
            refreshToken.IsRevoked = true;

            // Create new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.ID,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            return Ok(new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
        }

        // POST api/User/verify-email - Verify email with token
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return BadRequest("Invalid request.");

            var verificationToken = await _context.EmailVerificationTokens
                .FirstOrDefaultAsync(t => t.Token == dto.Token && t.UserId == user.ID && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

            if (verificationToken == null)
                return BadRequest("Invalid or expired token.");

            user.EmailVerified = true;
            verificationToken.IsUsed = true;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Email verified successfully." });
        }

        // POST api/User/resend-verification - Resend verification email
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || user.EmailVerified)
                return Ok(new { message = "If email exists and not verified, verification link will be sent." });

            var token = GenerateSecureToken();
            var verificationToken = new EmailVerificationToken
            {
                UserId = user.ID,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            _context.EmailVerificationTokens.Add(verificationToken);
            await _context.SaveChangesAsync();

            // TODO: Send email with verification link
            return Ok(new { message = "Verification link sent.", token = token });
        }

        private int GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedAccessException("User identity not found.");
            return int.Parse(value);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateSecureToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}



