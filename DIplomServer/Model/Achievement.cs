namespace DIplomServer.Model
{
    public class Achievement
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageIndex { get; set; } = "1";
        public int DefaultTarget { get; set; } = 1; // Добавляем поле с значением по умолчанию
    }
}