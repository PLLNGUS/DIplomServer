namespace DIplomServer.Model
{
    public class Habit
    {
        public int Id { get; set; } 
        public string Name { get; set; } 
        public string Description { get; set; } 
        public int Difficulty { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } 
        public string RepeatInterval { get; set; } 
        public string DaysOfWeek { get; set; }
        public string CategoryKey { get; set; } 

        public int UserId { get; set; } 

        public User? User { get; set; } 
    }
}
