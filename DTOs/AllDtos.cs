namespace QRend.API.DTOs
{
    // === AUTH ===
    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string? StudentNo { get; set; } // Öğrenci numara ile giriş
    }

    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public string Role { get; set; } = "";
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? StudentNo { get; set; }
    }

    // === DASHBOARD ===
    public class DashboardDto
    {
        public int LessonCount { get; set; }
        public int InstructorCount { get; set; }
        public int StudentCount { get; set; }
        public List<AtRiskStudentDto> AtRiskStudents { get; set; } = new();
        public List<AbsenceChartItemDto> AbsenceChartData { get; set; } = new();
    }

    public class AtRiskStudentDto
    {
        public string StudentName { get; set; } = "";
        public string LessonName { get; set; } = "";
        public int AbsenceCount { get; set; }
        public int MaxAbsence { get; set; }
        public int Remaining { get; set; }
    }

    public class AbsenceChartItemDto
    {
        public string LessonName { get; set; } = "";
        public int TotalAbsences { get; set; }
    }

    // === INSTRUCTOR ===
    public class InstructorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public int LessonCount { get; set; }
    }

    public class InstructorCreateDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Password { get; set; } = "123456";
    }

    public class InstructorUpdateDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
    }

    // === STUDENT ===
    public class StudentDto
    {
        public int Id { get; set; }
        public string StudentNo { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public class StudentCreateDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string StudentNo { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "123456";
    }

    public class StudentUpdateDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string StudentNo { get; set; } = "";
        public string Email { get; set; } = "";
    }

    // === LESSON ===
    public class LessonDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Code { get; set; }
        public string? Schedule { get; set; }
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = "";
        public int StudentCount { get; set; }
        public int MaxAbsence { get; set; }
    }

    public class LessonCreateDto
    {
        public string Name { get; set; } = "";
        public string? Code { get; set; }
        public string? Schedule { get; set; }
        public int InstructorId { get; set; }
        public int MaxAbsence { get; set; } = 14;
    }

    public class LessonUpdateDto
    {
        public string Name { get; set; } = "";
        public string? Code { get; set; }
        public string? Schedule { get; set; }
        public int InstructorId { get; set; }
        public int MaxAbsence { get; set; }
    }

    // === ATTENDANCE ===
    public class AttendanceStartDto
    {
        public int LessonId { get; set; }
    }

    public class AttendanceJoinDto
    {
        public string SessionToken { get; set; } = "";
    }

    public class AttendanceSessionDto
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string LessonName { get; set; } = "";
        public string LessonCode { get; set; } = "";
        public string SessionToken { get; set; } = "";
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public bool IsActive { get; set; }
        public List<AttendanceStudentDto> JoinedStudents { get; set; } = new();
    }

    public class AttendanceStudentDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentNo { get; set; } = "";
        public DateTime JoinedAt { get; set; }
    }

    public class AttendanceHistoryDto
    {
        public int SessionId { get; set; }
        public string LessonName { get; set; } = "";
        public string LessonCode { get; set; } = "";
        public DateTime Date { get; set; }
        public string StartedAt { get; set; } = "";
        public string EndedAt { get; set; } = "";
        public int JoinedCount { get; set; }
        public int TotalStudents { get; set; }
        public List<AttendanceStudentDto> JoinedStudents { get; set; } = new();
    }

    // === PROFILE ===
    public class ProfileDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public string? StudentNo { get; set; }
        public string Role { get; set; } = "";
    }

    public class ProfileUpdateDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }

    // === STUDENT COURSE VIEW ===
    public class StudentCourseDto
    {
        public int LessonId { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string InstructorName { get; set; } = "";
        public int TotalSessions { get; set; }
        public int AttendedSessions { get; set; }
        public int MaxAbsence { get; set; }
    }

    // === PAGED RESPONSE ===
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
