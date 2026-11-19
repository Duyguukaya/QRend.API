using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRend.API.Data;
using QRend.API.DTOs;
using QRend.API.Services;

namespace QRend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly AuthService _auth;

        public AuthController(AppDbContext db, AuthService auth)
        {
            _db = db;
            _auth = auth;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            Models.User? user = null;

            // Login by StudentNo (for mobile app)
            if (!string.IsNullOrEmpty(req.StudentNo))
            {
                user = await _db.Users.FirstOrDefaultAsync(u => u.StudentNo == req.StudentNo);
            }
            // Login by Email
            else if (!string.IsNullOrEmpty(req.Email))
            {
                user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            }

            if (user == null)
                return Unauthorized(new { message = "Kullanıcı bulunamadı." });

            var hash = SeedData.HashPassword(req.Password);
            if (user.PasswordHash != hash)
                return Unauthorized(new { message = "Şifre hatalı." });

            var token = _auth.GenerateToken(user);

            return Ok(new LoginResponse
            {
                Token = token,
                Role = user.Role,
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                StudentNo = user.StudentNo
            });
        }
    }
}
