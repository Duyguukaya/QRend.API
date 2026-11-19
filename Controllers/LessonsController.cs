using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRend.API.Data;
using QRend.API.DTOs;
using QRend.API.Models;
using System.Security.Claims;

namespace QRend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LessonsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public LessonsController(AppDbContext db) => _db = db;

        // Admin: tüm dersleri getir
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string search = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 6)
        {
            var query = _db.Lessons.Include(l => l.Instructor).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(l =>
                    l.Name.Contains(search) ||
                    (l.Instructor.FirstName + " " + l.Instructor.LastName).Contains(search));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var items = await query
                .OrderBy(l => l.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LessonDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Code = l.Code,
                    Schedule = l.Schedule,
                    InstructorId = l.InstructorId,
                    InstructorName = l.Instructor.FirstName + " " + l.Instructor.LastName,
                    StudentCount = l.LessonStudents.Count,
                    MaxAbsence = l.MaxAbsence
                })
                .ToListAsync();

            return Ok(new PagedResponse<LessonDto>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = page,
                TotalPages = totalPages
            });
        }

        // Eğitmen: kendi derslerini getir
        [HttpGet("my")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyLessons()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var lessons = await _db.Lessons
                .Where(l => l.InstructorId == userId)
                .Include(l => l.LessonStudents)
                .Select(l => new LessonDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Code = l.Code,
                    Schedule = l.Schedule,
                    InstructorId = l.InstructorId,
                    InstructorName = "",
                    StudentCount = l.LessonStudents.Count,
                    MaxAbsence = l.MaxAbsence
                })
                .ToListAsync();

            return Ok(lessons);
        }

        // Öğrenci: kendi derslerini ve devam durumunu getir
        [HttpGet("student")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentLessons()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var enrollments = await _db.LessonStudents
                .Where(ls => ls.StudentId == userId)
                .Include(ls => ls.Lesson)
                    .ThenInclude(l => l.Instructor)
                .Include(ls => ls.Lesson)
                    .ThenInclude(l => l.AttendanceSessions)
                .ToListAsync();

            var courses = new List<StudentCourseDto>();

            foreach (var enrollment in enrollments)
            {
                var lesson = enrollment.Lesson;
                var sessionIds = lesson.AttendanceSessions
                    .Where(s => !s.IsActive)
                    .Select(s => s.Id)
                    .ToList();

                var attended = await _db.AttendanceRecords
                    .CountAsync(ar => sessionIds.Contains(ar.SessionId) && ar.StudentId == userId);

                courses.Add(new StudentCourseDto
                {
                    LessonId = lesson.Id,
                    Name = lesson.Name,
                    Code = lesson.Code ?? "",
                    InstructorName = $"{lesson.Instructor.FirstName} {lesson.Instructor.LastName}",
                    TotalSessions = sessionIds.Count,
                    AttendedSessions = attended,
                    MaxAbsence = lesson.MaxAbsence
                });
            }

            return Ok(courses);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var lesson = await _db.Lessons
                .Include(l => l.Instructor)
                .Include(l => l.LessonStudents)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null) return NotFound();

            return Ok(new LessonDto
            {
                Id = lesson.Id,
                Name = lesson.Name,
                Code = lesson.Code,
                Schedule = lesson.Schedule,
                InstructorId = lesson.InstructorId,
                InstructorName = $"{lesson.Instructor.FirstName} {lesson.Instructor.LastName}",
                StudentCount = lesson.LessonStudents.Count,
                MaxAbsence = lesson.MaxAbsence
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] LessonCreateDto dto)
        {
            var instructor = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.InstructorId && u.Role == "Instructor");
            if (instructor == null)
                return BadRequest(new { message = "Geçersiz eğitmen." });

            var lesson = new Lesson
            {
                Name = dto.Name.Trim(),
                Code = dto.Code?.Trim(),
                Schedule = dto.Schedule?.Trim(),
                InstructorId = dto.InstructorId,
                MaxAbsence = dto.MaxAbsence
            };

            _db.Lessons.Add(lesson);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Ders başarıyla eklendi.", id = lesson.Id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] LessonUpdateDto dto)
        {
            var lesson = await _db.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();

            var instructor = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.InstructorId && u.Role == "Instructor");
            if (instructor == null)
                return BadRequest(new { message = "Geçersiz eğitmen." });

            lesson.Name = dto.Name.Trim();
            lesson.Code = dto.Code?.Trim();
            lesson.Schedule = dto.Schedule?.Trim();
            lesson.InstructorId = dto.InstructorId;
            lesson.MaxAbsence = dto.MaxAbsence;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Ders başarıyla güncellendi." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var lesson = await _db.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();

            _db.Lessons.Remove(lesson);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Ders başarıyla silindi." });
        }
    }
}
