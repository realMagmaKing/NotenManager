using NotenManager.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace NotenManager.Services
{
    public class DataService
    {
        private ObservableCollection<Subject> _subjects;
        private string _dataFilePath;
        private string _settingsFilePath;

        public DataService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "NotenManager");
            Directory.CreateDirectory(appFolder);

            _dataFilePath = Path.Combine(appFolder, "subjects.json");
            _settingsFilePath = Path.Combine(appFolder, "settings.json");

            LoadData();
        }

        private void LoadData()
        {
            if (File.Exists(_dataFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_dataFilePath);
                    var subjects = JsonSerializer.Deserialize<List<Subject>>(json);
                    _subjects = new ObservableCollection<Subject>(subjects ?? new List<Subject>());
                }
                catch
                {
                    InitializeData();
                }
            }
            else
            {
                InitializeData();
            }
        }

        public void SaveData()
        {
            try
            {
                var json = JsonSerializer.Serialize(_subjects.ToList());
                File.WriteAllText(_dataFilePath, json);
            }
            catch { }
        }

        private void InitializeData()
        {
            _subjects = new ObservableCollection<Subject>
            {
                new Subject
                {
                    Id = 1,
                    Name = "Mathematik",
                    LessonsPerWeek = 3,
                    Color = "math",
                    Notes = new List<Note>
                    {
                        new Note { Id = 1, Type = "Hausaufgabe", Grade = 2.3, Date = new DateTime(2024, 1, 15) },
                        new Note { Id = 2, Type = "Klassenarbeit", Grade = 2.8, Date = new DateTime(2024, 1, 22) },
                        new Note { Id = 3, Type = "Test", Grade = 3.5, Date = new DateTime(2024, 2, 5) }
                    }
                },
                new Subject
                {
                    Id = 2,
                    Name = "Biologie",
                    LessonsPerWeek = 4,
                    Color = "bio",
                    Notes = new List<Note>
                    {
                        new Note { Id = 1, Type = "Test", Grade = 3.1, Date = new DateTime(2024, 1, 20) }
                    }
                },
                new Subject
                {
                    Id = 3,
                    Name = "Informatik",
                    LessonsPerWeek = 2,
                    Color = "info",
                    Notes = new List<Note>
                    {
                        new Note { Id = 1, Type = "Projekt", Grade = 3.8, Date = new DateTime(2024, 1, 25) }
                    }
                },
                new Subject
                {
                    Id = 4,
                    Name = "Deutsch",
                    LessonsPerWeek = 4,
                    Color = "deutsch",
                    Notes = new List<Note>
                    {
                        new Note { Id = 1, Type = "Aufsatz", Grade = 2.5, Date = new DateTime(2024, 1, 18) }
                    }
                }
            };
        }

        public ObservableCollection<Subject> GetSubjects()
        {
            return _subjects;
        }

        public void AddSubject(Subject subject)
        {
            subject.Id = _subjects.Any() ? _subjects.Max(s => s.Id) + 1 : 1;
            _subjects.Add(subject);
            SaveData();
        }

        public void DeleteSubject(Subject subject)
        {
            _subjects.Remove(subject);
            SaveData();
        }

        public void AddNote(Subject subject, Note note)
        {
            note.Id = subject.Notes.Any() ? subject.Notes.Max(n => n.Id) + 1 : 1;
            subject.Notes.Add(note);
            SaveData();
        }

        public void UpdateNote(Subject subject, Note oldNote, Note newNote)
        {
            var index = subject.Notes.IndexOf(oldNote);
            if (index >= 0)
            {
                newNote.Id = oldNote.Id;
                subject.Notes[index] = newNote;
                SaveData();
            }
        }

        public void DeleteNote(Subject subject, Note note)
        {
            subject.Notes.Remove(note);
            SaveData();
        }

        public double GetOverallAverage()
        {
            var allNotes = _subjects.SelectMany(s => s.Notes).ToList();
            if (allNotes.Count == 0)
                return 0;
            return Math.Round(allNotes.Average(n => n.Grade), 1);
        }

        public AppSettings LoadSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch { }
            }
            return new AppSettings();
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch { }
        }

        public List<Note> GetRecentNotes(int count = 5)
        {
            return _subjects
                .SelectMany(s => s.Notes.Select(n => new { Subject = s, Note = n }))
                .OrderByDescending(x => x.Note.Date)
                .Take(count)
                .Select(x => x.Note)
                .ToList();
        }

        public Dictionary<string, int> GetGradeDistribution()
        {
            var allNotes = _subjects.SelectMany(s => s.Notes).ToList();
            var distribution = new Dictionary<string, int>
            {
                { "1.0-2.0", allNotes.Count(n => n.Grade >= 1.0 && n.Grade < 2.0) },
                { "2.0-3.0", allNotes.Count(n => n.Grade >= 2.0 && n.Grade < 3.0) },
                { "3.0-4.0", allNotes.Count(n => n.Grade >= 3.0 && n.Grade < 4.0) },
                { "4.0-5.0", allNotes.Count(n => n.Grade >= 4.0 && n.Grade < 5.0) },
                { "5.0-6.0", allNotes.Count(n => n.Grade >= 5.0 && n.Grade <= 6.0) }
            };
            return distribution;
        }
    }
}