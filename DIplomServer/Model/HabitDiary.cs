namespace DIplomServer.Model
{
    public class HabitDiary
    {
        public int Id { get; set; }

        // Связь с пользователем
        public int UserId { get; set; }
        public User User { get; set; }

        public int HabitId { get; set; }
        public Habit Habit { get; set; }

        public DateTime Date { get; set; } // Дата выполнения привычки/задачи
        public bool IsCompleted { get; set; } // Удалось выполнить привычку/задачу
        public string Notes { get; set; } // Примечания пользователя
    }
}
