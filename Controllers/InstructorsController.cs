using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRend.API.Data;
using QRend.API.DTOs;
using QRend.API.Models;

namespace QRend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class InstructorsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public InstructorsController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 6)
        {
            var query = _db.Users.Where(u => u.Role == "Instructor");

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    (u.FirstName + " " + u.LastName).Contains(search) ||
                    u.Email.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var items = await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new InstructorDto
                {
                    Id = u.Id,
                    Name = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    Phone = u.Phone ?? "",
                    LessonCount = _db.Lessons.Count(l => l.InstructorId == u.Id)
                })
                .ToListAsync();

            return Ok(new PagedResponse<InstructorDto>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = page,
                TotalPages = totalPages
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "Instructor");
            if (user == null) return NotFound();

            return Ok(new InstructorDto
            {
                Id = user.Id,
                Name = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                Phone = user.Phone ?? "",
                LessonCount = await _db.Lessons.CountAsync(l => l.InstructorId == user.Id)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InstructorCreateDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Bu e-posta adresi zaten kullanılıyor." });

            var user = new User
            {
                Email = dto.Email.Trim(),
                PasswordHash = SeedData.HashPassword(dto.Password),
                Role = "Instructor",
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Phone = dto.Phone?.Trim()
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Eğitmen başarıyla eklendi.", id = user.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] InstructorUpdateDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "Instructor");
            if (user == null) return NotFound();

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.Email = dto.Email.Trim();
            user.Phone = dto.Phone?.Trim();

            await _db.SaveChangesAsync();
            return Ok(new { message = "Eğitmen başarıyla güncellendi." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "Instructor");
            if (user == null) return NotFound();

            // Check if instructor has lessons
            if (await _db.Lessons.AnyAsync(l => l.InstructorId == id))
                return BadRequest(new { message = "Bu eğitmenin dersleri var, önce dersleri silin." });

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Eğitmen başarıyla silindi." });
        }
    }
}
