namespace DIplomServer.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int Level { get; set; } = 1;
        public int ExperiencePoints { get; set; } = 0;
        public string? ProfilePicture { get; set; }
        public int CurrentStreak { get; set; } = 0;
        public int MaxStreak { get; set; } = 0;
        public DateTime? LastStreakDate { get; set; }
        public string BorderStyle { get; set; } = "solid:#FF000000";

        public ICollection<UserAchievement>? UserAchievements { get; set; }

    }
}
