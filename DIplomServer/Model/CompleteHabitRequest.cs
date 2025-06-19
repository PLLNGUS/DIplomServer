namespace DIplomServer.Model
{
    public class CompleteHabitRequest
    {
        public int UserId { get; set; }
        public int HabitId { get; set; }
        public string? Notes { get; set; }  

    }
}
