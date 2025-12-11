namespace NotenManager.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int LessonsPerWeek { get; set; }
        public string Color { get; set; }
        public List<Note> Notes { get; set; } = new List<Note>();

        public double Average
        {
            get
            {
                if (Notes == null || Notes.Count == 0)
                    return 0;
                return Math.Round(Notes.Average(n => n.Grade), 1);
            }
        }
    }

    public class Note
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public double Grade { get; set; }
        public DateTime Date { get; set; }
    }

    public class AppSettings
    {
        public bool IsDarkMode { get; set; }
        public string UserName { get; set; }
        public string ClassName { get; set; }
        public bool ShowNotifications { get; set; }
        public bool ShowWeeklySummary { get; set; }
        public string TargetAverage { get; set; } = "3.0";
    }
}