using System.ComponentModel.DataAnnotations;

namespace QRend.API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        [Required, MaxLength(20)]
        public string Role { get; set; } = ""; // Admin, Instructor, Student

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = "";

        [Required, MaxLength(50)]
        public string LastName { get; set; } = "";

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(20)]
        public string? StudentNo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Lesson> InstructedLessons { get; set; } = new List<Lesson>();
        public ICollection<LessonStudent> EnrolledLessons { get; set; } = new List<LessonStudent>();
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }
}
