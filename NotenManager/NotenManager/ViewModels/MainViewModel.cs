using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotenManager.Models;
using NotenManager.Services;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Text.RegularExpressions;

namespace NotenManager.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private DataService _dataService;

        // snapshot of loaded grading settings to detect changes
        private double _originalMinGrade = 1.0;
        private double _originalMaxGrade = 6.0;
        private bool _originalIsAscending = false;
        private string _originalGradingSystem = "Schweiz (6-1, 6 = Beste)";

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
        private double newNoteGrade = 4.0;

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

        [ObservableProperty]
        private ISeries[] series;

        [ObservableProperty]
        private Axis[] xAxes;

        [ObservableProperty]
        private Axis[] yAxes;

        [ObservableProperty]
        private List<ChartLegendItem> chartLegendItems;

        // Validation error properties (null = no error)
        [ObservableProperty]
        private string newNoteTypeError;

        [ObservableProperty]
        private string newNoteGradeError;

        [ObservableProperty]
        private string newNoteDateError;

        [ObservableProperty]
        private string newSubjectNameError;

        [ObservableProperty]
        private string newSubjectLessonsError;

        [ObservableProperty]
        private string settingsUserNameError;

        [ObservableProperty]
        private string settingsClassNameError;

        [ObservableProperty]
        private string[] availableGradingSystems = new[] {
 "Schweiz (6-1, 6 = Beste)",
 "Deutschland (1-6, 1 = Beste)",
 "USA (A-F)",
 "Prozent (0-100%)",
 "Benutzerdefiniert"
 };

        [ObservableProperty]
        private string[] availableChartStyles = new[] { "Linie", "Balken", "Fl�che" };

        [ObservableProperty]
        private string notificationTimeDisplay = "18:00 Uhr";

        [ObservableProperty]
        private string gradeRangeLabel = "Note (6-1)";

        private Note _editingNote;

        // regex to allow letters (unicode), digits, spaces and a few safe punctuation characters
        private static readonly Regex AllowedNameRegex = new(@"^[\p{L}\d\s\-\.,()]+$", RegexOptions.Compiled);

        // Helper mappings for US letter grades
        private static readonly (double min, double max, string letter)[] UsLetterRanges = new[]
        {
 (3.7,4.0, "A"),
 (2.7,3.69, "B"),
 (1.7,2.69, "C"),
 (0.7,1.69, "D"),
 (0.0,0.69, "F")
 };

        private string NumericToLetter(double value)
        {
            // value expected0.0 -4.0
            foreach (var r in UsLetterRanges)
            {
                if (value >= r.min && value <= r.max)
                    return r.letter;
            }
            return "F";
        }

        private double LetterToNumeric(string letter)
        {
            if (string.IsNullOrWhiteSpace(letter))
                return 0.0;
            switch (letter.Trim().ToUpperInvariant())
            {
                case "A": return 4.0;
                case "B": return 3.0;
                case "C": return 2.0;
                case "D": return 1.0;
                case "F": return 0.0;
                default: return 0.0;
            }
        }

        public MainViewModel()
        {
            try
            {
                _data_service_constructor();
                // populate display grades based on loaded settings
                RefreshDisplayGrades();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainViewModel constructor error: {ex.Message}");
                // Initialize with defaults if loading fails
                _dataService = new DataService();
                Subjects = new ObservableCollection<Subject>();
                Settings = new AppSettings();
                RecentNotes = new List<Note>();
            }
        }

        private void _data_service_constructor()
        {
            _dataService = new DataService();
            Subjects = _dataService.GetSubjects();
            Settings = _data_service_or_load();
            IsDarkMode = Settings.IsDarkMode;

            // store snapshot of grading scale at load time
            _originalMinGrade = Settings.MinGrade;
            _originalMaxGrade = Settings.MaxGrade;
            _originalIsAscending = Settings.IsAscending;
            _originalGradingSystem = Settings.GradingSystem ?? "Schweiz (6-1, 6 = Beste)";

            UpdateNotificationTimeDisplay();
            UpdateOverallAverage();
            LoadRecentNotes();
            CheckTargetAverage();
            UpdateGradeRangeLabel();
        }

        private AppSettings _data_service_or_load()
        {
            try
            {
                var settings = _dataService.LoadSettings();
                return settings ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        private void UpdateNotificationTimeDisplay()
        {
            try
            {
                if (Settings != null)
                {
                    NotificationTimeDisplay = $"{Settings.NotificationTime:00}:00 Uhr";
                }
            }
            catch
            {
                NotificationTimeDisplay = "18:00 Uhr";
            }
        }

        // Update grade range label based on current settings
        private void UpdateGradeRangeLabel()
        {
            try
            {
                if (Settings == null)
                {
                    GradeRangeLabel = "Note";
                    return;
                }

                // Special handling for USA system
                if (Settings.GradingSystem?.StartsWith("USA") == true)
                {
                    GradeRangeLabel = "Note (A-F)";
                    return;
                }

                // Format label depending on ascending flag
                if (Settings.IsAscending)
                {
                    GradeRangeLabel = $"Note ({Settings.MinGrade}-{Settings.MaxGrade})";
                }
                else
                {
                    GradeRangeLabel = $"Note ({Settings.MaxGrade}-{Settings.MinGrade})";
                }
            }
            catch
            {
                GradeRangeLabel = "Note (6-1)";
            }
        }

        partial void OnSettingsChanged(AppSettings value)
        {
            if (value != null)
            {
                UpdateNotificationTimeDisplay();
                UpdateGradeRangeLabel();
            }
        }

        [RelayCommand]
        private void NavigateToOverview()
        {
            Series = null;
            XAxes = null;
            YAxes = null;
            ChartLegendItems = null;
            CurrentPage = "Overview";
            UpdateOverallAverage();
            LoadRecentNotes();
        }

        [RelayCommand]
        private void NavigateToSubjects()
        {
            Series = null;
            XAxes = null;
            YAxes = null;
            ChartLegendItems = null;
            CurrentPage = "Subjects";
        }

        [RelayCommand]
        private void NavigateToSettings()
        {
            Series = null;
            XAxes = null;
            YAxes = null;
            ChartLegendItems = null;
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
            UpdateGradeChart();
        }

        [RelayCommand]
        private void ShowAddSubjectModal()
        {
            NewSubjectName = "";
            NewSubjectLessons = 3;
            NewSubjectNameError = null;
            NewSubjectLessonsError = null;
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
            ValidateNewSubjectName();
            ValidateNewSubjectLessons();
   if (NewSubjectNameError != null || NewSubjectLessonsError != null)
         return;

     var colors = new[] { "math", "bio", "info", "deutsch", "other" };
      var newSubject = new Subject
        {
 Name = NewSubjectName.Trim(),
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
            "L�schen",
            $"M�chtest du das Fach '{subject.Name}' wirklich l�schen?",
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

            // Set default grade based on grading system
            if (Settings.GradingSystem?.StartsWith("USA") == true)
   {
      NewNoteGrade = 4.0; // A grade
      }
    else if (Settings.IsAscending)
      {
   NewNoteGrade = Settings.MinGrade; // Best grade for ascending systems (e.g., 1 for Germany)
   }
    else
            {
          NewNoteGrade = Settings.MaxGrade; // Best grade for descending systems (e.g., 6 for Switzerland)
     }

    NewNoteDate = DateTime.Now;
  // clear errors
    NewNoteTypeError = null;
NewNoteGradeError = null;
  NewNoteDateError = null;
    IsAddNoteModalVisible = true;
        }

        [RelayCommand]
        private void CloseAddNoteModal()
        {
            IsAddNoteModalVisible = false;
        }

        // Validate on property changes
        partial void OnNewNoteTypeChanged(string value)
        {
            ValidateNewNoteType();
        }

        partial void OnNewNoteGradeChanged(double value)
        {
            ValidateNewNoteGrade();
        }

        partial void OnNewNoteDateChanged(DateTime value)
        {
            ValidateNewNoteDate();
        }

        partial void OnNewSubjectNameChanged(string value)
        {
            ValidateNewSubjectName();
        }

        partial void OnNewSubjectLessonsChanged(int value)
        {
            ValidateNewSubjectLessons();
        }

        public void ValidateNewNoteType()
        {
            if (string.IsNullOrWhiteSpace(NewNoteType))
                NewNoteTypeError = "Bitte gib eine Notensart ein.";
            else if (NewNoteType.Trim().Length > 100)
                NewNoteTypeError = "Notensart ist zu lang.";
            else if (!AllowedNameRegex.IsMatch(NewNoteType.Trim()))
                NewNoteTypeError = "Notensart darf keine Sonderzeichen enthalten.";
            else
                NewNoteTypeError = null;
        }

        public void ValidateNewNoteGrade()
        {
            try
            {
         // Validate based on current grading system
       if (Settings == null)
      {
    NewNoteGradeError = "Einstellungen nicht geladen";
       return;
      }

        // Check for NaN or Infinity
       if (double.IsNaN(NewNoteGrade) || double.IsInfinity(NewNoteGrade))
      {
           NewNoteGradeError = "Ung�ltige Eingabe";
     return;
    }

         // Special validation for percentage system
      if (Settings.GradingSystem?.StartsWith("Prozent") == true)
         {
   if (NewNoteGrade < 0 || NewNoteGrade > 100)
          {
 NewNoteGradeError = "Prozent muss zwischen 0 und 100 liegen";
      return;
         }
        }
     else if (NewNoteGrade < Settings.MinGrade || NewNoteGrade > Settings.MaxGrade)
      {
          string rangeText;
  if (Settings.GradingSystem?.StartsWith("USA") == true)
   {
            rangeText = "A (4.0) bis F (0.0)";
      }
         else if (Settings.IsAscending)
        {
          rangeText = $"{Settings.MinGrade} (beste) bis {Settings.MaxGrade} (schlechteste)";
           }
          else
           {
               rangeText = $"{Settings.MinGrade} (schlechteste) bis {Settings.MaxGrade} (beste)";
                    }

    NewNoteGradeError = $"Note muss zwischen {rangeText} liegen";
         return;
     }

     NewNoteGradeError = null;
     }
  catch (Exception ex)
        {
        System.Diagnostics.Debug.WriteLine($"ValidateNewNoteGrade error: {ex.Message}");
 NewNoteGradeError = "Fehler bei der Validierung";
            }
        }

        public void ValidateNewNoteDate()
        {
            if (NewNoteDate.Date > DateTime.Now.Date)
                NewNoteDateError = "Datum darf nicht in der Zukunft liegen.";
            else
                NewNoteDateError = null;
        }

        public bool ValidateNewNoteAll()
        {
            ValidateNewNoteType();
            ValidateNewNoteGrade();
            ValidateNewNoteDate();
            return NewNoteTypeError == null && NewNoteGradeError == null && NewNoteDateError == null;
        }

        public void ValidateNewSubjectName()
        {
            if (string.IsNullOrWhiteSpace(NewSubjectName))
                NewSubjectNameError = "Bitte gib einen Fachnamen ein.";
            else if (NewSubjectName.Trim().Length > 50)
                NewSubjectNameError = "Fachname ist zu lang.";
            else if (!AllowedNameRegex.IsMatch(NewSubjectName.Trim()))
                NewSubjectNameError = "Fachname darf keine Sonderzeichen enthalten.";
            else if (Subjects != null && Subjects.Any(s => string.Equals(s.Name?.Trim(), NewSubjectName.Trim(), StringComparison.OrdinalIgnoreCase)))
                NewSubjectNameError = "Ein Fach mit diesem Namen existiert bereits.";
            else
                NewSubjectNameError = null;
        }

        public void ValidateNewSubjectLessons()
        {
            if (NewSubjectLessons < 1 || NewSubjectLessons > 20)
                NewSubjectLessonsError = "Lektionen pro Woche muss zwischen 1 und 20 liegen.";
            else
                NewSubjectLessonsError = null;
        }

        [RelayCommand]
        private void AddNote()
        {
 try
       {
      // perform validation and show inline errors instead of popup
if (!ValidateNewNoteAll())
       return;

       // Additional safety check for grade value
      if (double.IsNaN(NewNoteGrade) || double.IsInfinity(NewNoteGrade))
       {
     NewNoteGradeError = "Ung�ltige Note";
    return;
   }

      var newNote = new Note
   {
   Type = NewNoteType.Trim(),
   Grade = NewNoteGrade,
Date = NewNoteDate
  };
     // set display for USA
  if (Settings != null && Settings.GradingSystem != null && Settings.GradingSystem.StartsWith("USA"))
      {
     newNote.DisplayGrade = NumericToLetter(newNote.Grade);
      }

          _dataService.AddNote(SelectedSubject, newNote);

    // Force complete UI refresh
       var tempIndex = Subjects.IndexOf(SelectedSubject);
        var tempSubject = SelectedSubject;

     // Remove and re-add to force UI update
     Subjects.RemoveAt(tempIndex);
        Subjects.Insert(tempIndex, tempSubject);

    // Re-select the subject
       SelectedSubject = null;
     SelectedSubject = tempSubject;

     UpdateOverallAverage();
      LoadRecentNotes();
      CheckTargetAverage();
   UpdateGradeChart();
       IsAddNoteModalVisible = false;
     }
      catch (Exception ex)
   {
       System.Diagnostics.Debug.WriteLine($"AddNote error: {ex.Message}");
  Application.Current?.MainPage?.DisplayAlert("Fehler", "Fehler beim Hinzuf�gen der Note. Bitte �berpr�fe deine Eingaben.", "OK");
     }
  }

        [RelayCommand]
        private void ShowEditNoteModal(Note note)
        {
            _editingNote = note;
            NewNoteType = note.Type;
            NewNoteGrade = note.Grade;
            NewNoteDate = note.Date;
            // clear errors
            NewNoteTypeError = null;
            NewNoteGradeError = null;
            NewNoteDateError = null;
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
            try
   {
     if (!ValidateNewNoteAll())
    return;

       // Additional safety check for grade value
     if (double.IsNaN(NewNoteGrade) || double.IsInfinity(NewNoteGrade))
      {
        NewNoteGradeError = "Ung�ltige Note";
  return;
 }

       var updatedNote = new Note
       {
         Type = NewNoteType.Trim(),
        Grade = NewNoteGrade,
       Date = NewNoteDate
  };
  if (Settings != null && Settings.GradingSystem != null && Settings.GradingSystem.StartsWith("USA"))
      {
         updatedNote.DisplayGrade = NumericToLetter(updatedNote.Grade);
      }

     _dataService.UpdateNote(SelectedSubject, _editingNote, updatedNote);

     // Force complete UI refresh
       var tempIndex = Subjects.IndexOf(SelectedSubject);
     var tempSubject = SelectedSubject;

    // Remove and re-add to force UI update
      Subjects.RemoveAt(tempIndex);
       Subjects.Insert(tempIndex, tempSubject);

   // Re-select the subject
      SelectedSubject = null;
       SelectedSubject = tempSubject;

   UpdateOverallAverage();
        LoadRecentNotes();
   CheckTargetAverage();
      UpdateGradeChart();
    IsEditNoteModalVisible = false;
   }
      catch (Exception ex)
     {
      System.Diagnostics.Debug.WriteLine($"UpdateNote error: {ex.Message}");
  Application.Current?.MainPage?.DisplayAlert("Fehler", "Fehler beim Aktualisieren der Note. Bitte �berpr�fe deine Eingaben.", "OK");
     }
      }

        [RelayCommand]
        private async void DeleteNoteFromModal()
        {
            if (_editingNote == null) return;

            bool answer = await Application.Current.MainPage.DisplayAlert(
            "L�schen",
 "M�chtest du diese Note wirklich l�schen?",
     "Ja",
"Nein");

     if (answer)
     {
            _dataService.DeleteNote(SelectedSubject, _editingNote);

      // Force complete UI refresh
     var tempIndex = Subjects.IndexOf(SelectedSubject);
         var tempSubject = SelectedSubject;

   // Remove and re-add to force UI update
    Subjects.RemoveAt(tempIndex);
       Subjects.Insert(tempIndex, tempSubject);

   // Re-select the subject
    SelectedSubject = null;
   SelectedSubject = tempSubject;

         UpdateOverallAverage();
   LoadRecentNotes();
         CheckTargetAverage();
  UpdateGradeChart();
          IsEditNoteModalVisible = false;
            }
        }

        [RelayCommand]
        private async void DeleteNote(Note note)
        {
            bool answer = await Application.Current.MainPage.DisplayAlert(
            "L�schen",
"M�chtest du diese Note wirklich l�schen?",
          "Ja",
"Nein");

    if (answer)
   {
       _dataService.DeleteNote(SelectedSubject, note);

      // Force complete UI refresh
      var tempIndex = Subjects.IndexOf(SelectedSubject);
 var tempSubject = SelectedSubject;

     // Remove and re-add to force UI update
     Subjects.RemoveAt(tempIndex);
   Subjects.Insert(tempIndex, tempSubject);

   // Re-select the subject
  SelectedSubject = null;
      SelectedSubject = tempSubject;

    UpdateOverallAverage();
  LoadRecentNotes();
    CheckTargetAverage();
  UpdateGradeChart();
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
        private async void SaveSettings()
        {
            // validate settings (user name)
            ValidateSettingsUserName();
            if (SettingsUserNameError != null)
            {
                await Application.Current?.MainPage?.DisplayAlert("Fehler", SettingsUserNameError, "OK");
                return;
            }

            // Keep old snapshot values for conversion
            var oldMin = _originalMinGrade;
            var oldMax = _originalMaxGrade;
            var oldIsAsc = _originalIsAscending;
            var oldSystem = _originalGradingSystem;

            // Update grading scale based on selected system
            UpdateGradingScale();

            // If grading system or orientation changed, convert existing notes
            var newMin = Settings.MinGrade;
            var newMax = Settings.MaxGrade;
            var newIsAsc = Settings.IsAscending;
            var newSystem = Settings.GradingSystem;

            bool scaleChanged = oldMin != newMin || oldMax != newMax || oldIsAsc != newIsAsc || oldSystem != newSystem;

            if (scaleChanged)
            {
                try
                {
                    ConvertAllGrades(oldMin, oldMax, oldIsAsc, newMin, newMax, newIsAsc);
                    _dataService.SaveData();

                    // Force complete UI refresh
               var tempSubjects = Subjects.ToList();
     Subjects.Clear();
      foreach (var subject in tempSubjects)
        {
 Subjects.Add(subject);
          }

     // Update overall average
         UpdateOverallAverage();
      LoadRecentNotes();
       }
         catch (Exception ex)
       {
 await Application.Current?.MainPage?.DisplayAlert("Fehler", $"Fehler bei der Notenumrechnung: {ex.Message}", "OK");
               return;
     }
            }

            // Persist settings after conversion
            _dataService.SaveSettings(Settings);
            UpdateNotificationTimeDisplay();

    // update snapshot to new values
    _originalMinGrade = newMin;
            _originalMaxGrade = newMax;
            _originalIsAscending = newIsAsc;
_originalGradingSystem = newSystem;

    // Update grade range label after saving settings
            UpdateGradeRangeLabel();

     // Refresh display grades and charts after settings conversion
            RefreshDisplayGrades();

            if (CurrentPage == "Notes" && SelectedSubject != null)
{
             UpdateGradeChart();
    }

            await Application.Current?.MainPage?.DisplayAlert("Gespeichert", "Einstellungen wurden erfolgreich gespeichert.\n\nAlle Noten wurden umgerechnet.", "OK");
        }

        private void ConvertAllGrades(double oldMin, double oldMax, bool oldIsAsc, double newMin, double newMax, bool newIsAsc)
        {
        if (Subjects == null) return;

        double oldRange = oldMax - oldMin;
                double newRange = newMax - newMin;

        if (oldRange == 0 || newRange == 0)
            return;

        foreach (var subject in Subjects)
            {
    if (subject?.Notes == null) continue;

    foreach (var note in subject.Notes)
            {
        if (note == null) continue;

                double numericOld = note.Grade;

        // Step 1: Normalize to 0-1 scale (0 = worst, 1 = best)
                double normalizedValue;

      if (oldIsAsc)
     {
         // Lower is better (e.g., Germany 1=best, 6=worst)
         // Map: oldMin (best) -> 1.0, oldMax (worst) -> 0.0
 normalizedValue = (oldMax - numericOld) / oldRange;
          }
           else
           {
        // Higher is better (e.g., Switzerland 6=best, 1=worst or USA 4=best, 0=worst)
        // Map: oldMin (worst) -> 0.0, oldMax (best) -> 1.0
         normalizedValue = (numericOld - oldMin) / oldRange;
    }

            // Clamp to 0-1 range
        normalizedValue = Math.Clamp(normalizedValue, 0.0, 1.0);

 // Step 2: Convert from normalized 0-1 to new scale
                double newNumeric;

          if (newIsAsc)
        {
          // Lower is better in new system
        // Map: 1.0 (best) -> newMin, 0.0 (worst) -> newMax
   newNumeric = newMin + (1.0 - normalizedValue) * newRange;
           }
     else
     {
        // Higher is better in new system
       // Map: 1.0 (best) -> newMax, 0.0 (worst) -> newMin
              newNumeric = newMin + normalizedValue * newRange;
                 }

           // Round to one decimal place
 newNumeric = Math.Round(newNumeric, 1);

 // Ensure it's within bounds
     newNumeric = Math.Clamp(newNumeric, Math.Min(newMin, newMax), Math.Max(newMin, newMax));

               note.Grade = newNumeric;
        }
            }

            // Save and reload to ensure UI updates properly
            _dataService.SaveSubjects(Subjects);

            // Reload subjects to trigger property change notifications
            var reloadedSubjects = _dataService.GetSubjects();
            Subjects.Clear();
            foreach (var subject in reloadedSubjects)
            {
                Subjects.Add(subject);
            }

            // If we're on the Notes page, update the selected subject reference
            if (CurrentPage == "Notes" && SelectedSubject != null)
            {
                var updatedSubject = Subjects.FirstOrDefault(s => s.Id == SelectedSubject.Id);
                if (updatedSubject != null)
                {
                    SelectedSubject = updatedSubject;
                }
            }
      }

        private void RefreshDisplayGrades()
        {
            if (Settings == null || Subjects == null) return;

            try
            {
                foreach (var subject in Subjects)
                {
                    if (subject?.Notes == null) continue;

                    foreach (var note in subject.Notes)
                    {
                        if (note == null) continue;

                        if (!string.IsNullOrEmpty(Settings.GradingSystem) && Settings.GradingSystem.StartsWith("USA"))
                        {
                            note.DisplayGrade = NumericToLetter(note.Grade);
                        }
                        else
                        {
                            note.DisplayGrade = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshDisplayGrades error: {ex.Message}");
            }
        }

        private void UpdateGradingScale()
        {
            if (Settings == null) return;

            switch (Settings.GradingSystem)
            {
                case "Schweiz (6-1, 6 = Beste)":
                    Settings.MinGrade = 1.0;
                    Settings.MaxGrade = 6.0;
                    Settings.IsAscending = false;
                    break;
                case "Deutschland (1-6, 1 = Beste)":
                    Settings.MinGrade = 1.0;
                    Settings.MaxGrade = 6.0;
                    Settings.IsAscending = true;
                    break;
                case "USA (A-F)":
                    Settings.MinGrade = 0.0;
                    Settings.MaxGrade = 4.0;
                    Settings.IsAscending = true;
                    break;
                case "Prozent (0-100%)":
                    Settings.MinGrade = 0.0;
                    Settings.MaxGrade = 100.0;
                    Settings.IsAscending = true;
                    break;
                case "Benutzerdefiniert":
                    // Keep existing values
                    break;
            }
        }

        public void UpdateNotificationTime(int hour)
        {
            Settings.NotificationTime = hour;
            UpdateNotificationTimeDisplay();
        }

        [RelayCommand]
        private void ExportData()
        {
            Application.Current?.MainPage?.DisplayAlert("Export", "Export-Funktion wird in K�rze verf�gbar sein", "OK");
        }

        [RelayCommand]
        private async void ClearAllData()
        {
            bool answer = await Application.Current.MainPage.DisplayAlert(
            "Alle Daten l�schen",
            "M�chtest du wirklich ALLE Daten l�schen? Dies kann nicht r�ckg�ngig gemacht werden!",
            "Ja, l�schen",
            "Abbrechen");

            if (answer)
            {
                Subjects.Clear();
                _dataService.SaveData();
                UpdateOverallAverage();
                await Application.Current.MainPage.DisplayAlert("Gel�scht", "Alle Daten wurden gel�scht", "OK");
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

        private string MapSubjectColorToHex(string colorKey)
        {
            return colorKey switch
            {
                "math" => "#f5576c",
                "bio" => "#00f2fe",
                "info" => "#38f9d7",
                "deutsch" => "#fee140",
                _ => "#667eea",
            };
        }

        private void UpdateGradeChart()
        {
            // Reset first
            Series = null;
            XAxes = null;
            YAxes = null;
            ChartLegendItems = null;

            if (SelectedSubject == null || SelectedSubject.Notes.Count == 0)
            {
                return;
            }

            var sortedNotes = SelectedSubject.Notes.OrderBy(n => n.Date).ToList();

            // Create legend items with numbers and running average
            var runningSum = 0.0;
            var legendItems = new List<ChartLegendItem>();
            var values = new List<double>();

            for (int i = 0; i < sortedNotes.Count; i++)
            {
                var note = sortedNotes[i];
                runningSum += note.Grade;
                var runningAvg = runningSum / (i + 1);

                legendItems.Add(new ChartLegendItem
                {
                    Number = i + 1,
                    Note = note,
                    Average = Math.Round(runningAvg, 2)
                });

                values.Add(runningAvg);
            }

            ChartLegendItems = legendItems;

            var hex = MapSubjectColorToHex(SelectedSubject.Color);
            var color = SKColor.Parse(hex);

            // Create chart based on selected style
            switch (Settings.ChartStyle)
            {
                case "Balken":
                    Series = new ISeries[]
                    {
 new ColumnSeries<double>
 {
 Values = values,
 Name = "Durchschnitt",
 Fill = new SolidColorPaint(color),
 Stroke = null,
 MaxBarWidth = 40
 }
                    };
                    break;

                case "Fl�che":
                    Series = new ISeries[]
                    {
 new LineSeries<double>
 {
 Values = values,
 Name = "Durchschnitt",
 Fill = new SolidColorPaint(color.WithAlpha(100)),
 Stroke = new SolidColorPaint(color) { StrokeThickness = 3 },
 GeometrySize = 8,
 GeometryFill = new SolidColorPaint(color),
 GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
 LineSmoothness = 0.5
 }
                    };
                    break;

                case "Linie":
                default:
                    Series = new ISeries[]
                    {
 new LineSeries<double>
 {
 Values = values,
 Name = "Durchschnitt",
 Fill = null,
 Stroke = new SolidColorPaint(color) { StrokeThickness = 3 },
 GeometrySize = 0,
 LineSmoothness = 0.5
 }
                    };
                    break;
            }

            XAxes = new Axis[]
            {
 new Axis
 {
 Labels = sortedNotes.Select(n => n.Date.ToString("dd.MM")).ToArray(),
 TextSize = 12,
 SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 }
 }
            };

            YAxes = new Axis[]
            {
 new Axis
 {
 MinLimit = Settings.MinGrade,
 MaxLimit = Settings.MaxGrade,
 TextSize = 12,
 SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 }
 }
            };
        }

        partial void OnTargetAverageTextChanged(string value)
        {
            Settings.TargetAverage = value;
            CheckTargetAverage();
        }

        partial void OnSelectedSubjectChanged(Subject value)
        {
            // Reset chart properties
            Series = null;
            XAxes = null;
            YAxes = null;
            ChartLegendItems = null;

            // Create new chart for selected subject
            if (value != null && CurrentPage == "Notes")
            {
                UpdateGradeChart();
            }
        }

        public void ValidateSettingsUserName()
        {
            if (string.IsNullOrWhiteSpace(Settings.UserName))
            {
                SettingsUserNameError = null; // Optional field
                return;
            }

            if (Settings.UserName.Trim().Length > 50)
            {
                SettingsUserNameError = "Name ist zu lang.";
                return;
            }

            if (!AllowedNameRegex.IsMatch(Settings.UserName.Trim()))
            {
                SettingsUserNameError = "Name darf keine Sonderzeichen enthalten.";
                return;
            }

            SettingsUserNameError = null;
        }

        public void ValidateSettingsClassName()
        {
            if (string.IsNullOrWhiteSpace(Settings.ClassName))
{
    SettingsClassNameError = null; // Optional field
 return;
    }

  if (Settings.ClassName.Trim().Length > 20)
     {
      SettingsClassNameError = "Klassenname ist zu lang (max. 20 Zeichen)";
     return;
      }

   if (!AllowedNameRegex.IsMatch(Settings.ClassName.Trim()))
    {
  SettingsClassNameError = "Klassenname darf keine Sonderzeichen enthalten";
    return;
     }

 SettingsClassNameError = null;
        }

        [RelayCommand]
  private void NavigateToInfo()
    {
    CurrentPage = "Info";
  }

     [RelayCommand]
  private void NavigateToFaq()
 {
 CurrentPage = "Faq";
  }
    }
}