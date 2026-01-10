using NotenManager.ViewModels;
using Microsoft.Maui.ApplicationModel;
using System.Timers;

namespace NotenManager;

public partial class MainPage : ContentPage, IDisposable
{
    private MainViewModel _viewModel;
    private bool _isUpdatingGradeText = false;
    private System.Timers.Timer _gradeInputTimer;
    private string _pendingGradeText = "";
    private bool _disposed = false;

    public MainPage()
    {
        try
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            BindingContext = _viewModel;

            Application.Current.UserAppTheme = _viewModel.IsDarkMode ? AppTheme.Dark : AppTheme.Light;
            
            // Initialize debounce timer
            _gradeInputTimer = new System.Timers.Timer(300); // 300ms delay
            _gradeInputTimer.Elapsed += OnGradeInputTimerElapsed;
            _gradeInputTimer.AutoReset = false;
   
            // Subscribe to property changes to update UI mode
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainPage constructor error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Re-throw to show error to user
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            if (_gradeInputTimer != null)
            {
                _gradeInputTimer.Stop();
                _gradeInputTimer.Elapsed -= OnGradeInputTimerElapsed;
                _gradeInputTimer.Dispose();
            }

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dispose error: {ex.Message}");
        }

        _disposed = true;
    }

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.IsAddNoteModalVisible))
        {
            if (_viewModel.IsAddNoteModalVisible)
            {
                UpdateGradeInputMode(UsaGradePicker, StandardGradeEntry, GradeHintLabel);
            }
        }
        else if (e.PropertyName == nameof(_viewModel.IsEditNoteModalVisible))
        {
            if (_viewModel.IsEditNoteModalVisible)
            {
                UpdateGradeInputMode(UsaGradePickerEdit, StandardGradeEntryEdit, GradeHintLabelEdit);
                SetPickerFromGrade(UsaGradePickerEdit, _viewModel.NewNoteGrade);
            }
        }
    }

    private void UpdateGradeInputMode(Picker usaPicker, Entry standardEntry, Label hintLabel)
    {
        if (_viewModel.Settings?.GradingSystem?.StartsWith("USA") == true)
        {
            // USA Mode: Show Picker
            usaPicker.IsVisible = true;
            standardEntry.IsVisible = false;
            hintLabel.Text = "Wähle eine Buchstabennote (A-F)";
            SetPickerFromGrade(usaPicker, _viewModel.NewNoteGrade);
        }
        else if (_viewModel.Settings?.GradingSystem?.StartsWith("Prozent") == true)
        {
            // Percentage Mode
            usaPicker.IsVisible = false;
            standardEntry.IsVisible = true;
            standardEntry.Placeholder = "z.B. 85";
            hintLabel.Text = "Gib eine Prozentzahl zwischen 0 und 100 ein";
        }
        else if (_viewModel.Settings?.IsAscending == true)
        {
            // Ascending (e.g., Germany 1=best)
            usaPicker.IsVisible = false;
            standardEntry.IsVisible = true;
            standardEntry.Placeholder = $"z.B. {_viewModel.Settings.MinGrade}";
            hintLabel.Text = $"{_viewModel.Settings.MinGrade} = beste Note, {_viewModel.Settings.MaxGrade} = schlechteste Note";
        }
        else
        {
            // Descending (e.g., Swiss 6=best)
            usaPicker.IsVisible = false;
            standardEntry.IsVisible = true;
            standardEntry.Placeholder = $"z.B. {_viewModel.Settings.MaxGrade}";
            hintLabel.Text = $"{_viewModel.Settings.MaxGrade} = beste Note, {_viewModel.Settings.MinGrade} = schlechteste Note";
        }
    }

    private void SetPickerFromGrade(Picker picker, double grade)
    {
        // Map grade to picker index
        var index = grade switch
        {
            >= 3.85 => 0, // A (4.0)
            >= 3.5 => 1, // A- (3.7)
            >= 3.15 => 2, // B+ (3.3)
            >= 2.85 => 3, // B (3.0)
            >= 2.5 => 4, // B- (2.7)
            >= 2.15 => 5, // C+ (2.3)
            >= 1.85 => 6, // C (2.0)
            >= 1.5 => 7, // C- (1.7)
            >= 1.15 => 8, // D+ (1.3)
            >= 0.85 => 9, // D (1.0)
            >= 0.35 => 10, // D- (0.7)
            _ => 11 // F (0.0)
        };
        picker.SelectedIndex = index;
    }

    private async void OnLegendItemTapped(object sender, EventArgs e)
    {
        var grid = sender as Grid;
        var item = grid?.BindingContext as Models.ChartLegendItem;
        if (item == null) return;
        await DisplayAlert($"Note {item.Number}", $"Datum: {item.Note.Date:d}\nTyp: {item.Note.Type}\nNote: {item.Note.Grade:F1}\nDurchschnitt bis hier: {item.Average:F2}", "OK");
    }

    private void OnDarkModeToggled(object sender, ToggledEventArgs e)
    {
        Application.Current.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
        _viewModel.Settings.IsDarkMode = e.Value;
    }

    // Validation event handlers invoked from XAML
    private void OnNewSubjectName_TextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel.ValidateNewSubjectName();
    }

    private void OnNewNoteType_TextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel.ValidateNewNoteType();
    }

    private void OnSettingsUserName_TextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel.ValidateSettingsUserName();
    }

    private void OnSettingsClassName_TextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel.ValidateSettingsClassName();
    }

    private void OnNewNoteGrade_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Prevent recursive calls
        if (_isUpdatingGradeText)
          return;

        var entry = sender as Entry;
     if (entry == null) return;

        // Store pending text and restart timer
_pendingGradeText = e.NewTextValue ?? "";
  
  // Stop any existing timer
        if (_gradeInputTimer != null)
   {
      _gradeInputTimer.Stop();
  _gradeInputTimer.Start();
        }
      
        // Also do immediate cleaning for better UX
 try
 {
   ProcessGradeInput(entry, _pendingGradeText, false);
        }
      catch (Exception ex)
   {
      System.Diagnostics.Debug.WriteLine($"OnNewNoteGrade_TextChanged error: {ex.Message}");
    }
    }

    private void OnGradeInputTimerElapsed(object sender, ElapsedEventArgs e)
    {
        // Timer elapsed - process the final value
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                // Safely get the active entry
                Entry entry = null;
      
                if (StandardGradeEntry != null && StandardGradeEntry.IsVisible)
                {
                    entry = StandardGradeEntry;
                }
                else if (StandardGradeEntryEdit != null && StandardGradeEntryEdit.IsVisible)
                {
                    entry = StandardGradeEntryEdit;
                }
               
                // Only process if we have a valid entry
                if (entry != null)
                {
                    ProcessGradeInput(entry, _pendingGradeText, true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnGradeInputTimerElapsed error: {ex.Message}");
            }
        });
    }

    private void ProcessGradeInput(Entry entry, string inputText, bool isFinal)
    {
        if (_isUpdatingGradeText)
            return;

        try
        {
            _isUpdatingGradeText = true;

            if (string.IsNullOrEmpty(inputText))
            {
                _viewModel.NewNoteGrade = 0;
                _viewModel.ValidateNewNoteGrade();
                return;
            }

            // Clean the input
            var cleanedText = "";
            bool hasDecimalPoint = false;
            
            foreach (char c in inputText)
            {
                if (char.IsDigit(c))
                {
                    cleanedText += c;
                }
                else if ((c == '.' || c == ',') && !hasDecimalPoint)
                {
                    cleanedText += '.';
                    hasDecimalPoint = true;
                }
            }

            // Remove leading zeros
            if (cleanedText.Length > 1 && cleanedText[0] == '0' && cleanedText[1] != '.')
            {
                cleanedText = cleanedText.TrimStart('0');
                if (string.IsNullOrEmpty(cleanedText))
                    cleanedText = "0";
            }

            if (string.IsNullOrEmpty(cleanedText) || cleanedText == ".")
            {
                if (isFinal)
                {
                    _viewModel.NewNoteGrade = 0;
                    _viewModel.ValidateNewNoteGrade();
                }
                return;
            }

            // Parse the value
            if (!double.TryParse(cleanedText, System.Globalization.NumberStyles.Float, 
                System.Globalization.CultureInfo.InvariantCulture, out double grade))
            {
                return;
            }

            // Get max allowed value
            double maxAllowed = 100;
            if (_viewModel.Settings != null)
            {
                if (_viewModel.Settings.GradingSystem?.StartsWith("Prozent") == true)
                {
                    maxAllowed = 100;
                }
                else
                {
                    maxAllowed = Math.Max(_viewModel.Settings.MinGrade, _viewModel.Settings.MaxGrade);
                }
            }

            // Cap the value
            bool wasCapped = false;
            if (grade > maxAllowed)
            {
                grade = maxAllowed;
                wasCapped = true;
            }

            // Update ViewModel
            _viewModel.NewNoteGrade = grade;
            _viewModel.ValidateNewNoteGrade();

            // Update Entry text only if needed and only on final or if capped
            if (isFinal || wasCapped)
          {
 string formattedText;
    
                // Format based on grading system
 if (_viewModel.Settings?.GradingSystem?.StartsWith("Prozent") == true)
 {
        // Percentage: no decimal places (e.g., "85")
         formattedText = grade.ToString("F0", System.Globalization.CultureInfo.InvariantCulture);
     }
           else
    {
             // Swiss/German/USA: show decimal only if needed
    // If grade is whole number (e.g., 5.0), show as "5"
      // If grade has decimal (e.g., 5.5), show as "5.5"
       if (grade == Math.Floor(grade))
      {
        // Whole number - no decimal
     formattedText = grade.ToString("F0", System.Globalization.CultureInfo.InvariantCulture);
        }
          else
         {
  // Has decimal - show one decimal place
            formattedText = grade.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
   }
  }
      
    if (entry.Text != formattedText)
        {
           entry.Text = formattedText;
  }
         }
        }
        finally
        {
            _isUpdatingGradeText = false;
        }
    }

    private void OnNewNoteDate_DateSelected(object sender, DateChangedEventArgs e)
    {
        _viewModel.NewNoteDate = e.NewDate;
        _viewModel.ValidateNewNoteDate();
    }

    private void OnNotificationTimeChanged(object sender, ValueChangedEventArgs e)
    {
        // Round to nearest hour for cleaner display
        var roundedValue = (int)Math.Round(e.NewValue);
        if (Math.Abs(NotificationSlider.Value - roundedValue) > 0.1)
        {
            NotificationSlider.Value = roundedValue;
        }
        _viewModel.UpdateNotificationTime(roundedValue);
    }

    private void OnUsaGradePickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        if (picker == null || picker.SelectedIndex < 0) return;

      var gradeValue = picker.SelectedIndex switch
        {
   0 => 4.0, // A
     1 => 3.7, // A-
     2 => 3.3, // B+
          3 => 3.0, // B
            4 => 2.7, // B-
       5 => 2.3, // C+
            6 => 2.0, // C
            7 => 1.7, // C-
  8 => 1.3, // D+
      9 => 1.0, // D
10 => 0.7, // D-
        11 => 0.0, // F
      _ => 2.0
        };

    _viewModel.NewNoteGrade = gradeValue;
   _viewModel.ValidateNewNoteGrade();
    }

    // FAQ Collapse/Expand handlers
    private void OnFaq1Tapped(object sender, EventArgs e)
    {
        Faq1Answer.IsVisible = !Faq1Answer.IsVisible;
Faq1Arrow.Text = Faq1Answer.IsVisible ? "▲" : "▼";
    }

    private void OnFaq2Tapped(object sender, EventArgs e)
    {
    Faq2Answer.IsVisible = !Faq2Answer.IsVisible;
     Faq2Arrow.Text = Faq2Answer.IsVisible ? "▲" : "▼";
    }

    private void OnFaq3Tapped(object sender, EventArgs e)
    {
        Faq3Answer.IsVisible = !Faq3Answer.IsVisible;
        Faq3Arrow.Text = Faq3Answer.IsVisible ? "▲" : "▼";
    }

    private void OnFaq4Tapped(object sender, EventArgs e)
    {
        Faq4Answer.IsVisible = !Faq4Answer.IsVisible;
  Faq4Arrow.Text = Faq4Answer.IsVisible ? "▲" : "▼";
    }

    private void OnFaq5Tapped(object sender, EventArgs e)
    {
        Faq5Answer.IsVisible = !Faq5Answer.IsVisible;
        Faq5Arrow.Text = Faq5Answer.IsVisible ? "▲" : "▼";
    }
}