using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotenManager.Models;
using NotenManager.Services;
using System.Collections.ObjectModel;

namespace NotenManager.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DataService _dataService;

        [ObservableProperty]
        private string currentPage = "Overview";

        [ObservableProperty]
        private Subject selectedSubject;

        [ObservableProperty]
        private ObservableCollection<Subject> subjects;

        [ObservableProperty]
        private double overallAverage;

        [ObservableProperty]
        private bool isAddSubjectModalVisible;

        [ObservableProperty]
        private bool isAddNoteModalVisible;

        [ObservableProperty]
        private bool isEditNoteModalVisible;

        [ObservableProperty]
        private string newSubjectName = "";

        [ObservableProperty]
        private int newSubjectLessons = 3;

        [ObservableProperty]
        private string newNoteType = "";

        [ObservableProperty]
        private double newNoteGrade;

        [ObservableProperty]
        private DateTime newNoteDate = DateTime.Now;

        [ObservableProperty]
        private AppSettings settings;

        [ObservableProperty]
        private bool isDarkMode;

        [ObservableProperty]
        private List<Note> recentNotes;

        [ObservableProperty]
        private string targetAverageText = "3.0";

        [ObservableProperty]
        private bool targetReached;

        private Note _editingNote;

        public MainViewModel()
        {
            _dataService = new DataService();
            Subjects = _dataService.GetSubjects();
            Settings = _dataService.LoadSettings();
            IsDarkMode = Settings.IsDarkMode;
            UpdateOverallAverage();
            LoadRecentNotes();
            CheckTargetAverage();
        }

        [RelayCommand]
        private void NavigateToOverview()
        {
            CurrentPage = "Overview";
            UpdateOverallAverage();
            LoadRecentNotes();
        }

        [RelayCommand]
        private void NavigateToSubjects()
        {
            CurrentPage = "Subjects";
        }

        [RelayCommand]
        private void NavigateToSettings()
        {
            CurrentPage = "Settings";
        }

        [RelayCommand]
        private void NavigateToStatistics()
        {
            CurrentPage = "Statistics";
        }

        [RelayCommand]
        private void NavigateToNotes(Subject subject)
        {
            SelectedSubject = subject;
            CurrentPage = "Notes";
        }

        [RelayCommand]
        private void ShowAddSubjectModal()
        {
            NewSubjectName = "";
            NewSubjectLessons = 3;
            IsAddSubjectModalVisible = true;
        }

        [RelayCommand]
        private void CloseAddSubjectModal()
        {
            IsAddSubjectModalVisible = false;
        }

        [RelayCommand]
        private void AddSubject()
        {
            if (string.IsNullOrWhiteSpace(NewSubjectName))
            {
                Application.Current?.MainPage?.DisplayAlert("Fehler", "Bitte gib einen Fachnamen ein", "OK");
                return;
            }

            var colors = new[] { "math", "bio", "info", "deutsch", "other" };
            var newSubject = new Subject
            {
                Name = NewSubjectName,
                LessonsPerWeek = NewSubjectLessons,
                Color = colors[Subjects.Count % colors.Length]
            };

            _dataService.AddSubject(newSubject);
            UpdateOverallAverage();
            IsAddSubjectModalVisible = false;
        }

        [RelayCommand]
        private async void DeleteSubject(Subject subject)
        {
            bool answer = await Application.Current.MainPage.DisplayAlert(
                "Löschen",
                $"Möchtest du das Fach '{subject.Name}' wirklich löschen?",
                "Ja",
                "Nein");

            if (answer)
            {
                _dataService.DeleteSubject(subject);
                UpdateOverallAverage();
            }
        }

        [RelayCommand]
        private void ShowAddNoteModal()
        {
            NewNoteType = "";
            NewNoteGrade = 0;
            NewNoteDate = DateTime.Now;
            IsAddNoteModalVisible = true;
        }

        [RelayCommand]
        private void CloseAddNoteModal()
        {
            IsAddNoteModalVisible = false;
        }

        [RelayCommand]
        private void AddNote()
        {
            if (string.IsNullOrWhiteSpace(NewNoteType))
            {
                Application.Current?.MainPage?.DisplayAlert("Fehler", "Bitte gib eine Notensart ein", "OK");
                return;
            }

            if (NewNoteGrade < 1 || NewNoteGrade > 6)
            {
                Application.Current?.MainPage?.DisplayAlert("Fehler", "Note muss zwischen 1 und 6 liegen", "OK");
                return;
            }

            var newNote = new Note
            {
                Type = NewNoteType,
                Grade = NewNoteGrade,
                Date = NewNoteDate
            };

            _dataService.AddNote(SelectedSubject, newNote);
            OnPropertyChanged(nameof(SelectedSubject));
            UpdateOverallAverage();
            LoadRecentNotes();
            CheckTargetAverage();
            IsAddNoteModalVisible = false;
        }

        [RelayCommand]
        private void ShowEditNoteModal(Note note)
        {
            _editingNote = note;
            NewNoteType = note.Type;
            NewNoteGrade = note.Grade;
            NewNoteDate = note.Date;
            IsEditNoteModalVisible = true;
        }

        [RelayCommand]
        private void CloseEditNoteModal()
        {
            IsEditNoteModalVisible = false;
            _editingNote = null;
        }

        [RelayCommand]
        private void UpdateNote()
        {
            if (string.IsNullOrWhiteSpace(NewNoteType))
            {
                Application.Current?.MainPage?.DisplayAlert("Fehler", "Bitte gib eine Notensart ein", "OK");
                return;
            }

            if (NewNoteGrade < 1 || NewNoteGrade > 6)
            {
                Application.Current?.MainPage?.DisplayAlert("Fehler", "Note muss zwischen 1 und 6 liegen", "OK");
                return;
            }

            var updatedNote = new Note
            {
                Type = NewNoteType,
                Grade = NewNoteGrade,
                Date = NewNoteDate
            };

            _dataService.UpdateNote(SelectedSubject, _editingNote, updatedNote);
            OnPropertyChanged(nameof(SelectedSubject));
            UpdateOverallAverage();
            LoadRecentNotes();
            CheckTargetAverage();
            IsEditNoteModalVisible = false;
        }

        [RelayCommand]
        private async void DeleteNoteFromModal()
        {
            if (_editingNote == null) return;

            bool answer = await Application.Current.MainPage.DisplayAlert(
                "Löschen",
                "Möchtest du diese Note wirklich löschen?",
                "Ja",
                "Nein");

            if (answer)
            {
                _dataService.DeleteNote(SelectedSubject, _editingNote);
                OnPropertyChanged(nameof(SelectedSubject));
                UpdateOverallAverage();
                LoadRecentNotes();
                CheckTargetAverage();
                IsEditNoteModalVisible = false;
            }
        }

        [RelayCommand]
        private async void DeleteNote(Note note)
        {
            bool answer = await Application.Current.MainPage.DisplayAlert(
                "Löschen",
                "Möchtest du diese Note wirklich löschen?",
                "Ja",
                "Nein");

            if (answer)
            {
                _dataService.DeleteNote(SelectedSubject, note);
                OnPropertyChanged(nameof(SelectedSubject));
                UpdateOverallAverage();
                LoadRecentNotes();
                CheckTargetAverage();
            }
        }

        [RelayCommand]
        private void ToggleDarkMode()
        {
            IsDarkMode = !IsDarkMode;
            Settings.IsDarkMode = IsDarkMode;
            _dataService.SaveSettings(Settings);

            Application.Current.UserAppTheme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
        }

        [RelayCommand]
        private void SaveSettings()
        {
            _dataService.SaveSettings(Settings);
            Application.Current?.MainPage?.DisplayAlert("Gespeichert", "Einstellungen wurden gespeichert", "OK");
        }

        [RelayCommand]
        private void ExportData()
        {
            Application.Current?.MainPage?.DisplayAlert("Export", "Export-Funktion wird in Kürze verfügbar sein", "OK");
        }

        [RelayCommand]
        private async void ClearAllData()
        {
            bool answer = await Application.Current.MainPage.DisplayAlert(
                "Alle Daten löschen",
                "Möchtest du wirklich ALLE Daten löschen? Dies kann nicht rückgängig gemacht werden!",
                "Ja, löschen",
                "Abbrechen");

            if (answer)
            {
                Subjects.Clear();
                _dataService.SaveData();
                UpdateOverallAverage();
                await Application.Current.MainPage.DisplayAlert("Gelöscht", "Alle Daten wurden gelöscht", "OK");
            }
        }

        private void UpdateOverallAverage()
        {
            OverallAverage = _dataService.GetOverallAverage();
        }

        private void LoadRecentNotes()
        {
            RecentNotes = _dataService.GetRecentNotes(5);
        }

        private void CheckTargetAverage()
        {
            if (double.TryParse(Settings.TargetAverage, out double target))
            {
                TargetReached = OverallAverage <= target;
            }
        }

        partial void OnTargetAverageTextChanged(string value)
        {
            Settings.TargetAverage = value;
            CheckTargetAverage();
        }
    }
}