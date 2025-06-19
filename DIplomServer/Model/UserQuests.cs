namespace DIplomServer.Model
{
    public class UserQuest
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int QuestId { get; set; }
        public Quest Quest { get; set; }

        public int CurrentProgress { get; set; }
        public int TargetProgress { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? DateCompleted { get; set; }
    }


}
