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
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public StudentsController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 8)
        {
            var query = _db.Users.Where(u => u.Role == "Student");

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    (u.FirstName + " " + u.LastName).Contains(search) ||
                    (u.StudentNo ?? "").Contains(search) ||
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
                .Select(u => new StudentDto
                {
                    Id = u.Id,
                    StudentNo = u.StudentNo ?? "",
                    Name = u.FirstName + " " + u.LastName,
                    Email = u.Email
                })
                .ToListAsync();

            return Ok(new PagedResponse<StudentDto>
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
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "Student");
            if (user == null) return NotFound();

            return Ok(new StudentDto
            {
                Id = user.Id,
                StudentNo = user.StudentNo ?? "",
                Name = $"{user.FirstName} {user.LastName}",
                Email = user.Email
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StudentCreateDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Bu e-posta adresi zaten kullanılıyor." });

            if (await _db.Users.AnyAsync(u => u.StudentNo == dto.StudentNo))
                return BadRequest(new { message = "Bu öğrenci numarası zaten kullanılıyor." });

            var user = new User
            {
                Email = dto.Email.Trim(),
                PasswordHash = SeedData.HashPassword(dto.Password),
                Role = "Student",
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                StudentNo = dto.StudentNo.Trim()
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Öğrenci başarıyla eklendi.", id = user.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StudentUpdateDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "Student");
            if (user == null) return NotFound();

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.Email = dto.Email.Trim();
            user.StudentNo = dto.StudentNo.Trim();

            await _db.SaveChangesAsync();
            return Ok(new { message = "Öğrenci başarıyla güncellendi." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "Student");
            if (user == null) return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Öğrenci başarıyla silindi." });
        }
    }
}
