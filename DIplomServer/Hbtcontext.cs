using DIplomServer.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace DIplomServer
{
    public class HbtContext : DbContext
    {
        public HbtContext(DbContextOptions<HbtContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitDiary> HabitDiaries { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; } 
        public DbSet<HabitLog> HabitLogs { get; set; }
        public DbSet<Quest> Quests { get; set; }
        public DbSet<QuestTemplate> QuestTemplates { get; set; }
        public DbSet<UserQuest> UserQuests { get; set; }
        




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
                entity.Property(a => a.ImageIndex).HasDefaultValue("1");
                entity.Property(a => a.DefaultTarget).IsRequired().HasDefaultValue(1); // Добавляем DefaultTarget
                                                                                       });
                modelBuilder.Entity<UserAchievement>(entity =>
            {
                entity.HasKey(ua => ua.Id);

                entity.HasOne(ua => ua.User)
                      .WithMany(u => u.UserAchievements)
                      .HasForeignKey(ua => ua.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ua => ua.Achievement)
                      .WithMany()
                      .HasForeignKey(ua => ua.AchievementId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(ua => ua.IsCompleted).HasDefaultValue(false);
                entity.Property(ua => ua.CurrentProgress).HasDefaultValue(0);
            });
        }
    }
}
