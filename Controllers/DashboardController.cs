using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRend.API.Data;
using QRend.API.DTOs;

namespace QRend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lessonCount = await _db.Lessons.CountAsync();
            var instructorCount = await _db.Users.CountAsync(u => u.Role == "Instructor");
            var studentCount = await _db.Users.CountAsync(u => u.Role == "Student");

            // Absence chart: per lesson, total sessions - attended = absences
            var lessons = await _db.Lessons
                .Include(l => l.AttendanceSessions)
                .Include(l => l.LessonStudents)
                .ToListAsync();

            var absenceChart = new List<AbsenceChartItemDto>();
            var atRiskStudents = new List<AtRiskStudentDto>();

            foreach (var lesson in lessons)
            {
                var sessionIds = lesson.AttendanceSessions.Select(s => s.Id).ToList();
                var totalSessions = sessionIds.Count;
                if (totalSessions == 0) continue;

                var studentIds = lesson.LessonStudents.Select(ls => ls.StudentId).ToList();
                var totalExpected = totalSessions * studentIds.Count;

                var actualAttendance = await _db.AttendanceRecords
                    .CountAsync(ar => sessionIds.Contains(ar.SessionId));

                absenceChart.Add(new AbsenceChartItemDto
                {
                    LessonName = lesson.Name,
                    TotalAbsences = totalExpected - actualAttendance
                });

                // At-risk students
                foreach (var studentId in studentIds)
                {
                    var attended = await _db.AttendanceRecords
                        .CountAsync(ar => sessionIds.Contains(ar.SessionId) && ar.StudentId == studentId);
                    var absences = totalSessions - attended;
                    var remaining = lesson.MaxAbsence - absences;

                    if (remaining <= 3 && remaining >= 0)
                    {
                        var student = await _db.Users.FindAsync(studentId);
                        atRiskStudents.Add(new AtRiskStudentDto
                        {
                            StudentName = student != null ? $"{student.FirstName} {student.LastName}" : "?",
                            LessonName = lesson.Name,
                            AbsenceCount = absences,
                            MaxAbsence = lesson.MaxAbsence,
                            Remaining = remaining
                        });
                    }
                }
            }

            return Ok(new DashboardDto
            {
                LessonCount = lessonCount,
                InstructorCount = instructorCount,
                StudentCount = studentCount,
                AbsenceChartData = absenceChart,
                AtRiskStudents = atRiskStudents.OrderBy(x => x.Remaining).Take(10).ToList()
            });
        }
    }
}
