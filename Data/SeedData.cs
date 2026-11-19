using Microsoft.EntityFrameworkCore;
using QRend.API.Models;
using System.Security.Cryptography;
using System.Text;

namespace QRend.API.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            // Ensure database created
            context.Database.Migrate();

            // If already seeded, skip
            if (context.Users.Any()) return;

            // --- USERS ---
            var admin = new User
            {
                Email = "admin@qrend.com",
                PasswordHash = HashPassword("Admin123!"),
                Role = "Admin",
                FirstName = "Admin",
                LastName = "Kullanıcı",
                Phone = "0500 000 00 00"
            };

            var instructors = new List<User>
            {
                new() { Email = "ahmet.yilmaz@universite.edu.tr", PasswordHash = HashPassword("123456"), Role = "Instructor", FirstName = "Ahmet", LastName = "Yılmaz", Phone = "0532 111 22 33" },
                new() { Email = "fatma.kaya@universite.edu.tr", PasswordHash = HashPassword("123456"), Role = "Instructor", FirstName = "Fatma", LastName = "Kaya", Phone = "0533 222 33 44" },
                new() { Email = "mehmet@qrend.app", PasswordHash = HashPassword("123456"), Role = "Instructor", FirstName = "Mehmet", LastName = "Demir", Phone = "0534 333 44 55" },
                new() { Email = "zeynep.sahin@universite.edu.tr", PasswordHash = HashPassword("123456"), Role = "Instructor", FirstName = "Zeynep", LastName = "Şahin", Phone = "0535 444 55 66" },
                new() { Email = "ali.celik@universite.edu.tr", PasswordHash = HashPassword("123456"), Role = "Instructor", FirstName = "Ali", LastName = "Çelik", Phone = "0536 555 66 77" },
            };

            var students = new List<User>
            {
                new() { Email = "ali.vural@ogrenci.edu.tr", PasswordHash = HashPassword("123456"), Role = "Student", FirstName = "Ali", LastName = "Vural", StudentNo = "20210101" },
                new() { Email = "selin.korkmaz@ogrenci.edu.tr", PasswordHash = HashPassword("123456"), Role = "Student", FirstName = "Selin", LastName = "Korkmaz", StudentNo = "20210102" },
                new() { Email = "mert.aksoy@ogrenci.edu.tr", PasswordHash = HashPassword("123456"), Role = "Student", FirstName = "Mert", LastName = "Aksoy", StudentNo = "20210103" },
                new() { Email = "busra.yildiz@ogrenci.edu.tr", PasswordHash = HashPassword("123456"), Role = "Student", FirstName = "Büşra", LastName = "Yıldız", StudentNo = "20210104" },
                new() { Email = "kaan.dogan@ogrenci.edu.tr", PasswordHash = HashPassword("123456"), Role = "Student", FirstName = "Kaan", LastName = "Doğan", StudentNo = "20210105" },
                new() { Email = "elif.arslan@ogrenci.edu.tr", PasswordHash = HashPassword("123456"), Role = "Student", FirstName = "Elif", LastName = "Arslan", StudentNo = "20210106" },
                new() { Email = "emre.cetin@ogrenci.edu.tr", PasswordHash = HashPassword("123456"), Role = "Student", FirstName = "Emre", LastName = "Çetin", StudentNo = "20210107" },
                new() { Email = "nazli.ozturk@ogrenci.edu.tr", PasswordHash = HashPassword("123456"), Role = "Student", FirstName = "Nazlı", LastName = "Öztürk", StudentNo = "20210108" },
                new() { Email = "baris.kilic@ogrenci.edu.tr", PasswordHash = HashPassword("123456"), Role = "Student", FirstName = "Barış", LastName = "Kılıç", StudentNo = "20210109" },
                new() { Email = "irem.gunes@ogrenci.edu.tr", PasswordHash = HashPassword("123456"), Role = "Student", FirstName = "İrem", LastName = "Güneş", StudentNo = "20210110" },
                new() { Email = "tolga.aydin@ogrenci.edu.tr", PasswordHash = HashPassword("123456"), Role = "Student", FirstName = "Tolga", LastName = "Aydın", StudentNo = "20210111" },
                new() { Email = "ceren.yilmaz@ogrenci.edu.tr", PasswordHash = HashPassword("123456"), Role = "Student", FirstName = "Ceren", LastName = "Yılmaz", StudentNo = "20210112" },
                new() { Email = "oguz.demir@ogrenci.edu.tr", PasswordHash = HashPassword("123456"), Role = "Student", FirstName = "Oğuz", LastName = "Demir", StudentNo = "20210113" },
                new() { Email = "ayse.polat@ogrenci.edu.tr", PasswordHash = HashPassword("123456"), Role = "Student", FirstName = "Ayşe", LastName = "Polat", StudentNo = "20210114" },
                new() { Email = "yunus.cakir@ogrenci.edu.tr", PasswordHash = HashPassword("123456"), Role = "Student", FirstName = "Yunus", LastName = "Çakır", StudentNo = "20210115" },
            };

            // Flutter test student — login with studentNo "12345"
            students.Add(new User
            {
                Email = "test@ogrenci.edu.tr",
                PasswordHash = HashPassword("password"),
                Role = "Student",
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                StudentNo = "12345"
            });

            context.Users.Add(admin);
            context.Users.AddRange(instructors);
            context.Users.AddRange(students);
            context.SaveChanges();

            // --- LESSONS ---
            var lessons = new List<Lesson>
            {
                new() { Name = "Veri Yapıları", Code = "CS201", Schedule = "Pazartesi 09:00 - 12:00", InstructorId = instructors[0].Id, MaxAbsence = 14 },
                new() { Name = "Algoritma Analizi", Code = "CS202", Schedule = "Salı 09:00 - 12:00", InstructorId = instructors[1].Id, MaxAbsence = 14 },
                new() { Name = "Web Programlama", Code = "CS301", Schedule = "Çarşamba 09:00 - 12:00", InstructorId = instructors[2].Id, MaxAbsence = 14 },
                new() { Name = "Veritabanı Yönetimi", Code = "CS302", Schedule = "Perşembe 09:00 - 12:00", InstructorId = instructors[3].Id, MaxAbsence = 14 },
                new() { Name = "Nesne Yönelimli Programlama", Code = "CS203", Schedule = "Cuma 09:00 - 12:00", InstructorId = instructors[0].Id, MaxAbsence = 14 },
                new() { Name = "İşletim Sistemleri", Code = "CS303", Schedule = "Pazartesi 13:00 - 16:00", InstructorId = instructors[4].Id, MaxAbsence = 14 },
                new() { Name = "Bilgisayar Ağları", Code = "CS304", Schedule = "Salı 13:00 - 16:00", InstructorId = instructors[1].Id, MaxAbsence = 14 },
                new() { Name = "Yazılım Mühendisliği", Code = "CS401", Schedule = "Çarşamba 13:00 - 16:00", InstructorId = instructors[2].Id, MaxAbsence = 14 },
            };

            context.Lessons.AddRange(lessons);
            context.SaveChanges();

            // --- LESSON STUDENTS (enroll all regular students to all lessons) ---
            var allStudentIds = students.Select(s => s.Id).ToList();
            var enrollments = new List<LessonStudent>();
            foreach (var lesson in lessons)
            {
                foreach (var studentId in allStudentIds)
                {
                    enrollments.Add(new LessonStudent { LessonId = lesson.Id, StudentId = studentId });
                }
            }
            context.LessonStudents.AddRange(enrollments);
            context.SaveChanges();

            // --- ATTENDANCE SESSIONS (past sessions for demo) ---
            var random = new Random(42);
            var sessions = new List<AttendanceSession>();
            foreach (var lesson in lessons)
            {
                for (int week = 1; week <= 10; week++)
                {
                    var sessionDate = DateTime.UtcNow.AddDays(-((14 - week) * 7));
                    var session = new AttendanceSession
                    {
                        LessonId = lesson.Id,
                        SessionToken = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                        StartedAt = sessionDate,
                        EndedAt = sessionDate.AddHours(3),
                        IsActive = false,
                        CreatedByUserId = lesson.InstructorId
                    };
                    sessions.Add(session);
                }
            }
            context.AttendanceSessions.AddRange(sessions);
            context.SaveChanges();

            // --- ATTENDANCE RECORDS (random attendance) ---
            var records = new List<AttendanceRecord>();
            foreach (var session in sessions)
            {
                foreach (var studentId in allStudentIds)
                {
                    // ~80% attendance rate
                    if (random.NextDouble() < 0.8)
                    {
                        records.Add(new AttendanceRecord
                        {
                            SessionId = session.Id,
                            StudentId = studentId,
                            JoinedAt = session.StartedAt.AddMinutes(random.Next(0, 15))
                        });
                    }
                }
            }
            context.AttendanceRecords.AddRange(records);
            context.SaveChanges();
        }

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
