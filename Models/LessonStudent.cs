using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRend.API.Models
{
    public class LessonStudent
    {
        [Key]
        public int Id { get; set; }

        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; } = null!;

        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public User Student { get; set; } = null!;

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }
}
