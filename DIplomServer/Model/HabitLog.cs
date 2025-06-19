using System;

namespace DIplomServer.Model
{
    public class HabitLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? HabitId { get; set; }
        public string Action { get; set; } // "Added", "Completed", "Deleted", "Skipped", и т.д.
        public DateTime Timestamp { get; set; }
        public string? Notes { get; set; }
    }
}
