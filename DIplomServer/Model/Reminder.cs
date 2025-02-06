namespace DIplomServer.Model
{
    public class Reminder
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int TaskId { get; set; }
        public DateTime ReminderTime { get; set; }
        public bool IsCompleted { get; set; }

        public DailyTask DailyTask { get; set; }
    }
}
