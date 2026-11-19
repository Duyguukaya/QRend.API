using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRend.API.Data;
using QRend.API.DTOs;
using System.Security.Claims;

namespace QRend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ProfileController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            return Ok(new ProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                StudentNo = user.StudentNo,
                Role = user.Role
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ProfileUpdateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Password change
            if (!string.IsNullOrEmpty(dto.CurrentPassword))
            {
                var currentHash = SeedData.HashPassword(dto.CurrentPassword);
                if (user.PasswordHash != currentHash)
                    return BadRequest(new { message = "Mevcut şifre yanlış." });

                if (string.IsNullOrEmpty(dto.NewPassword) || dto.NewPassword.Length < 6)
                    return BadRequest(new { message = "Yeni şifre en az 6 karakter olmalıdır." });

                user.PasswordHash = SeedData.HashPassword(dto.NewPassword);
            }

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.Email = dto.Email.Trim();
            user.Phone = dto.Phone?.Trim();

            await _db.SaveChangesAsync();
            return Ok(new { message = "Profil başarıyla güncellendi." });
        }
    }
}
