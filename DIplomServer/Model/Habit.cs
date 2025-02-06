namespace DIplomServer.Model
{
    public class Habit
    {
        public int Id { get; set; } // Уникальный идентификатор привычки
        public string Name { get; set; } // Название привычки
        public string Description { get; set; } // Описание привычки
        public int Difficulty { get; set; } // Сложность привычки (1-3)
        public DateTime StartDate { get; set; } // Дата начала привычки
        public DateTime? EndDate { get; set; } // Дата окончания привычки (не обязательное поле)
        public string RepeatInterval { get; set; } // Интервал повторения (например, "каждый день", "каждую неделю")
        public string DaysOfWeek { get; set; } // Дни недели (например, "Понедельник, Среда, Пятница")
        public int UserId { get; set; } // Идентификатор пользователя, которому принадлежит привычка

        public User User { get; set; } // Связь с пользователем
    }
}
