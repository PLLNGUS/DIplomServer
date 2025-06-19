namespace DIplomServer.Model
{
    public class UserAchievement
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int AchievementId { get; set; }
        public Achievement Achievement { get; set; }

        public bool IsCompleted { get; set; }
        public DateTime? DateReceived { get; set; }

        public int CurrentProgress { get; set; }  // Текущий прогресс
        public int TargetProgress { get; set; }   // Необходимый прогресс
    }
}
