using Microsoft.EntityFrameworkCore;
using QRend.API.Models;

namespace QRend.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Lesson> Lessons => Set<Lesson>();
        public DbSet<LessonStudent> LessonStudents => Set<LessonStudent>();
        public DbSet<AttendanceSession> AttendanceSessions => Set<AttendanceSession>();
        public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Unique student number (nullable)
            modelBuilder.Entity<User>()
                .HasIndex(u => u.StudentNo)
                .IsUnique()
                .HasFilter("[StudentNo] IS NOT NULL");

            // Lesson -> Instructor
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Instructor)
                .WithMany(u => u.InstructedLessons)
                .HasForeignKey(l => l.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // LessonStudent unique combo
            modelBuilder.Entity<LessonStudent>()
                .HasIndex(ls => new { ls.LessonId, ls.StudentId })
                .IsUnique();

            modelBuilder.Entity<LessonStudent>()
                .HasOne(ls => ls.Lesson)
                .WithMany(l => l.LessonStudents)
                .HasForeignKey(ls => ls.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LessonStudent>()
                .HasOne(ls => ls.Student)
                .WithMany(u => u.EnrolledLessons)
                .HasForeignKey(ls => ls.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // AttendanceSession -> Lesson
            modelBuilder.Entity<AttendanceSession>()
                .HasOne(a => a.Lesson)
                .WithMany(l => l.AttendanceSessions)
                .HasForeignKey(a => a.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AttendanceSession>()
                .HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // AttendanceRecord unique per session per student
            modelBuilder.Entity<AttendanceRecord>()
                .HasIndex(ar => new { ar.SessionId, ar.StudentId })
                .IsUnique();

            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(ar => ar.Session)
                .WithMany(s => s.AttendanceRecords)
                .HasForeignKey(ar => ar.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(ar => ar.Student)
                .WithMany(u => u.AttendanceRecords)
                .HasForeignKey(ar => ar.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
