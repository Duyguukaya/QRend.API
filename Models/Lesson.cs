using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRend.API.Models
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        [MaxLength(20)]
        public string? Code { get; set; }

        [MaxLength(100)]
        public string? Schedule { get; set; }

        public int InstructorId { get; set; }

        [ForeignKey("InstructorId")]
        public User Instructor { get; set; } = null!;

        public int MaxAbsence { get; set; } = 14;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<LessonStudent> LessonStudents { get; set; } = new List<LessonStudent>();
        public ICollection<AttendanceSession> AttendanceSessions { get; set; } = new List<AttendanceSession>();
    }
}
