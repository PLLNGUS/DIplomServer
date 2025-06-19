namespace DIplomServer.Model
{
    public class Quest
    {
        public int Id { get; set; }

        public int TemplateId { get; set; }
        public QuestTemplate Template { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int CurrentProgress { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
        public bool IsRewardClaimed { get; set; } = false;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
