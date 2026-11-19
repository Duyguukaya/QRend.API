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
    public class AttendanceController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AttendanceController(AppDbContext db) => _db = db;

        // Eğitmen: yoklama oturumu başlat
        [HttpPost("start")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Start([FromBody] AttendanceStartDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == dto.LessonId && l.InstructorId == userId);
            if (lesson == null)
                return BadRequest(new { message = "Bu ders size ait değil veya bulunamadı." });

            // Check if there's already an active session for this lesson
            var existing = await _db.AttendanceSessions.FirstOrDefaultAsync(s => s.LessonId == dto.LessonId && s.IsActive);
            if (existing != null)
                return BadRequest(new { message = "Bu ders için zaten aktif bir yoklama oturumu var." });

            var session = new AttendanceSession
            {
                LessonId = dto.LessonId,
                SessionToken = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                StartedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedByUserId = userId
            };

            _db.AttendanceSessions.Add(session);
            await _db.SaveChangesAsync();

            return Ok(new AttendanceSessionDto
            {
                Id = session.Id,
                LessonId = session.LessonId,
                LessonName = lesson.Name,
                LessonCode = lesson.Code ?? "",
                SessionToken = session.SessionToken,
                StartedAt = session.StartedAt,
                IsActive = true,
                JoinedStudents = new()
            });
        }

        // Eğitmen: yoklama durdur
        [HttpPost("stop/{sessionId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Stop(int sessionId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var session = await _db.AttendanceSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.CreatedByUserId == userId && s.IsActive);

            if (session == null)
                return NotFound(new { message = "Aktif oturum bulunamadı." });

            session.IsActive = false;
            session.EndedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Yoklama oturumu durduruldu." });
        }

        // Eğitmen: aktif oturumu getir
        [HttpGet("active")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetActive([FromQuery] int? lessonId = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var query = _db.AttendanceSessions
                .Where(s => s.CreatedByUserId == userId && s.IsActive);

            if (lessonId.HasValue)
                query = query.Where(s => s.LessonId == lessonId.Value);

            var session = await query
                .Include(s => s.Lesson)
                .Include(s => s.AttendanceRecords)
                    .ThenInclude(ar => ar.Student)
                .FirstOrDefaultAsync();

            if (session == null)
                return Ok((AttendanceSessionDto?)null);

            return Ok(new AttendanceSessionDto
            {
                Id = session.Id,
                LessonId = session.LessonId,
                LessonName = session.Lesson.Name,
                LessonCode = session.Lesson.Code ?? "",
                SessionToken = session.SessionToken,
                StartedAt = session.StartedAt,
                IsActive = true,
                JoinedStudents = session.AttendanceRecords.Select(ar => new AttendanceStudentDto
                {
                    StudentId = ar.StudentId,
                    StudentName = $"{ar.Student.FirstName} {ar.Student.LastName}",
                    StudentNo = ar.Student.StudentNo ?? "",
                    JoinedAt = ar.JoinedAt
                }).ToList()
            });
        }

        // Eğitmen: yoklama geçmişi
        [HttpGet("history")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetHistory([FromQuery] int? lessonId = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var baseQuery = _db.AttendanceSessions
                .Where(s => s.CreatedByUserId == userId && !s.IsActive);

            if (lessonId.HasValue)
                baseQuery = baseQuery.Where(s => s.LessonId == lessonId.Value);

            var sessions = await baseQuery
                .Include(s => s.Lesson)
                    .ThenInclude(l => l.LessonStudents)
                .Include(s => s.AttendanceRecords)
                    .ThenInclude(ar => ar.Student)
                .OrderByDescending(s => s.StartedAt)
                .Take(20)
                .ToListAsync();

            var history = sessions.Select(s => new AttendanceHistoryDto
            {
                SessionId = s.Id,
                LessonName = s.Lesson.Name,
                LessonCode = s.Lesson.Code ?? "",
                Date = s.StartedAt,
                StartedAt = s.StartedAt.ToString("HH:mm:ss"),
                EndedAt = s.EndedAt?.ToString("HH:mm:ss") ?? "",
                JoinedCount = s.AttendanceRecords.Count,
                TotalStudents = s.Lesson.LessonStudents.Count,
                JoinedStudents = s.AttendanceRecords.Select(ar => new AttendanceStudentDto
                {
                    StudentId = ar.StudentId,
                    StudentName = $"{ar.Student.FirstName} {ar.Student.LastName}",
                    StudentNo = ar.Student.StudentNo ?? "",
                    JoinedAt = ar.JoinedAt
                }).ToList()
            }).ToList();

            return Ok(history);
        }

        // Öğrenci: QR ile yoklamaya katıl
        [HttpPost("join")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Join([FromBody] AttendanceJoinDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var session = await _db.AttendanceSessions
                .Include(s => s.Lesson)
                .FirstOrDefaultAsync(s => s.SessionToken == dto.SessionToken && s.IsActive);

            if (session == null)
                return BadRequest(new { message = "Geçersiz veya süresi dolmuş yoklama kodu." });

            // Check if student is enrolled in this lesson
            var enrolled = await _db.LessonStudents.AnyAsync(
                ls => ls.LessonId == session.LessonId && ls.StudentId == userId);
            if (!enrolled)
                return BadRequest(new { message = "Bu derse kayıtlı değilsiniz." });

            // Check if already joined
            var alreadyJoined = await _db.AttendanceRecords.AnyAsync(
                ar => ar.SessionId == session.Id && ar.StudentId == userId);
            if (alreadyJoined)
                return BadRequest(new { message = "Bu yoklamaya zaten katıldınız." });

            var record = new AttendanceRecord
            {
                SessionId = session.Id,
                StudentId = userId,
                JoinedAt = DateTime.UtcNow
            };

            _db.AttendanceRecords.Add(record);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Yoklama başarılı!",
                lessonName = session.Lesson.Name,
                lessonCode = session.Lesson.Code ?? ""
            });
        }

        // Eğitmen: manuel öğrenci ekle
        [HttpPost("add-student")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> AddStudent([FromQuery] int sessionId, [FromQuery] int studentId)
        {
            var session = await _db.AttendanceSessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.IsActive);
            if (session == null) return NotFound(new { message = "Aktif oturum bulunamadı." });

            var alreadyJoined = await _db.AttendanceRecords.AnyAsync(
                ar => ar.SessionId == sessionId && ar.StudentId == studentId);
            if (alreadyJoined) return BadRequest(new { message = "Bu öğrenci zaten katılmış." });

            _db.AttendanceRecords.Add(new AttendanceRecord
            {
                SessionId = sessionId,
                StudentId = studentId,
                JoinedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return Ok(new { message = "Öğrenci yoklamaya eklendi." });
        }

        // Eğitmen: dersin öğrenci listesi
        [HttpGet("lesson-students/{lessonId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetLessonStudents(int lessonId)
        {
            var students = await _db.LessonStudents
                .Where(ls => ls.LessonId == lessonId)
                .Include(ls => ls.Student)
                .Select(ls => new StudentDto
                {
                    Id = ls.Student.Id,
                    StudentNo = ls.Student.StudentNo ?? "",
                    Name = ls.Student.FirstName + " " + ls.Student.LastName,
                    Email = ls.Student.Email
                })
                .ToListAsync();

            return Ok(students);
        }
    }
}
