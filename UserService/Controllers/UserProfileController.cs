using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Data;
using UserService.DTOs;
using UserService.Models;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly appDataContext _context;

        public UserProfileController(appDataContext context)
        {
            _context = context;
        }

        // GET api/userprofile — get logged-in user's profile
        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetUserId();
            var profile = await _context.UserProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return NotFound("Profile not found. Please create your travel profile first.");

            return Ok(MapToResponse(profile));
        }

        // GET api/userprofile/{userId} — internal use by BookingService
        [HttpGet("{userId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfileByUserId(int userId)
        {
            var profile = await _context.UserProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return NotFound();

            return Ok(MapToResponse(profile));
        }

        // POST api/userprofile — create profile
        [HttpPost]
        public async Task<IActionResult> CreateProfile([FromBody] UserProfileDto dto)
        {
            var userId = GetUserId();

            if (await _context.UserProfiles.AnyAsync(p => p.UserId == userId))
                return Conflict("Profile already exists. Use PUT to update.");

            var profile = new UserProfile
            {
                UserId = userId,
                PassportNumber = dto.PassportNumber,
                Gender = dto.Gender,
                Age = dto.Age,
                Nationality = dto.Nationality,
                DateOfBirth = dto.DateOfBirth
            };

            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyProfile), MapToResponse(profile));
        }

        // PUT api/userprofile — update profile
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto dto)
        {
            var userId = GetUserId();
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return NotFound("Profile not found. Use POST to create one.");

            profile.PassportNumber = dto.PassportNumber;
            profile.Gender = dto.Gender;
            profile.Age = dto.Age;
            profile.Nationality = dto.Nationality;
            profile.DateOfBirth = dto.DateOfBirth;

            await _context.SaveChangesAsync();
            return Ok(MapToResponse(profile));
        }

        // DELETE api/userprofile — delete profile
        [HttpDelete]
        public async Task<IActionResult> DeleteProfile()
        {
            var userId = GetUserId();
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return NotFound("Profile not found.");

            _context.UserProfiles.Remove(profile);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private int GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedAccessException("User identity not found.");
            return int.Parse(value);
        }

        private static UserProfileResponseDto MapToResponse(UserProfile p) => new()
        {
            UserId = p.UserId,
            FullName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}" : string.Empty,
            Email = p.User?.Email ?? string.Empty,
            PassportNumber = p.PassportNumber,
            Gender = p.Gender,
            Age = p.Age,
            Nationality = p.Nationality,
            DateOfBirth = p.DateOfBirth
        };
    }
}
