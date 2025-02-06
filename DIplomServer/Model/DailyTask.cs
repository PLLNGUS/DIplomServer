namespace DIplomServer.Model
{
    public class DailyTask
    {
        public int Id { get; set; } // Уникальный идентификатор
        public string Name { get; set; } // Название дела
        public string Description { get; set; } // Описание дела
        public DateTime Date { get; set; } // Дата выполнения дела
        public bool IsCompleted { get; set; } // Статус выполнения
        public int Priority { get; set; } // Приоритет выполнения дела
        public string Category { get; set; } // Категория дела (например, "Работа", "Здоровье")
        public int UserId { get; set; } // Идентификатор пользователя

        public User User { get; set; } // Связь с пользователем
    }
}
