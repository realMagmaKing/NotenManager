using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System;
using System.Linq;

namespace NotenManager.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int LessonsPerWeek { get; set; }
        public string Color { get; set; }
        public ObservableCollection<Note> Notes { get; set; } = new ObservableCollection<Note>();

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

        // For USA system, store the numeric value internally but display as letter
        [System.Text.Json.Serialization.JsonIgnore]
        public string DisplayGrade { get; set; }
    }

    public class ChartLegendItem
    {
        public int Number { get; set; }
        public Note Note { get; set; }
        public double Average { get; set; }
    }

    public class AppSettings
    {
        public bool IsDarkMode { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public bool ShowNotifications { get; set; }
        public bool ShowWeeklySummary { get; set; }
        public string TargetAverage { get; set; } = "3.0";
        // default must match picker strings
        public string GradingSystem { get; set; } = "Schweiz (6-1, 6 = Beste)";
        public int NotificationTime { get; set; } = 18; // Hour (0-23)
        public string ChartStyle { get; set; } = "Linie"; // Linie, Balken, Fläche
        public double MinGrade { get; set; } = 1.0;
        public double MaxGrade { get; set; } = 6.0;
        public bool IsAscending { get; set; } = false; // false =6 ist beste Note (Schweiz), true =1 ist beste Note (Deutschland)
    }
}