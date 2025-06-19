namespace DIplomServer.Model
{
    public class HabitDiary
    {
        public int Id { get; set; }

        
        public int UserId { get; set; }
        public User User { get; set; }

        public int HabitId { get; set; }
        public Habit Habit { get; set; }

        public DateTime Date { get; set; } 
        public bool IsCompleted { get; set; } 
        public string? Notes { get; set; } 
    }
}
