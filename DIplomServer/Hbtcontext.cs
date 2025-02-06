using DIplomServer.Model;
using Microsoft.EntityFrameworkCore;

namespace DIplomServer
{
    public class HbtContext : DbContext
    {
        public HbtContext(DbContextOptions<HbtContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Habit> Habits { get; set; }
        public DbSet<DailyTask> DailyTasks { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<HabitDiary> HabitDiaries { get; set; }
        public DbSet<Achievement> Achievements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Nickname).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.Password).IsRequired();
                entity.Property(u => u.Level).HasDefaultValue(1);
                entity.Property(u => u.ProfilePicture).HasColumnType("bytea");
            });

            // Конфигурация Habit
            modelBuilder.Entity<Habit>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Name).IsRequired().HasMaxLength(100);
                entity.Property(h => h.Description).HasMaxLength(255);
                entity.Property(h => h.Difficulty).IsRequired();
                entity.Property(h => h.RepeatInterval).HasMaxLength(50);
                entity.Property(h => h.DaysOfWeek).HasMaxLength(100);

                entity.HasOne(h => h.User)
                      .WithMany()
                      .HasForeignKey(h => h.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация DailyTask
            modelBuilder.Entity<DailyTask>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
                entity.Property(d => d.Description).HasMaxLength(255);
                entity.Property(d => d.Category).HasMaxLength(50);

                entity.HasOne(d => d.User)
                      .WithMany()
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация Reminder
            modelBuilder.Entity<Reminder>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Title).IsRequired().HasMaxLength(100);
                entity.Property(r => r.IsCompleted).HasDefaultValue(false);

                entity.HasOne(r => r.DailyTask)
                      .WithMany()
                      .HasForeignKey(r => r.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация HabitDiary
            modelBuilder.Entity<HabitDiary>(entity =>
            {
                entity.HasKey(hd => hd.Id);
                entity.Property(hd => hd.Date).IsRequired();
                entity.Property(hd => hd.Notes).HasMaxLength(255);

                entity.HasOne(hd => hd.User)
                      .WithMany()
                      .HasForeignKey(hd => hd.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(hd => hd.Habit)
                      .WithMany()
                      .HasForeignKey(hd => hd.HabitId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация Achievement
            modelBuilder.Entity<Achievement>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Title).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Description).HasMaxLength(255);
                entity.Property(a => a.DateReceived).IsRequired();

                entity.HasOne(a => a.User)
                      .WithMany()
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
