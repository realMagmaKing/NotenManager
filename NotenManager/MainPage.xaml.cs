using NotenManager.ViewModels;
using Microsoft.Maui.ApplicationModel;

namespace NotenManager;

public partial class MainPage : ContentPage
{
    private MainViewModel _viewModel;

    public MainPage()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        BindingContext = _viewModel;

        Application.Current.UserAppTheme = _viewModel.IsDarkMode ? AppTheme.Dark : AppTheme.Light;
        
        // Subscribe to property changes to update UI mode
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
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

    private void OnNewNoteGrade_TextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = sender as Entry;
        if (entry == null) return;

        var newText = e.NewTextValue ?? "";
        
        // Allow empty input
        if (string.IsNullOrEmpty(newText))
        {
            _viewModel.NewNoteGrade = 0;
            _viewModel.ValidateNewNoteGrade();
            return;
        }

        // Remove all non-numeric characters except ONE decimal separator
        // Allow only: digits (0-9), dot (.), and comma (,)
        var cleanedText = "";
        bool hasDecimalPoint = false;
        
        foreach (char c in newText)
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
            // Skip any other character (including spaces, letters, additional dots)
        }

        // Prevent leading zeros (except "0." for decimals)
        if (cleanedText.Length > 1 && cleanedText[0] == '0' && cleanedText[1] != '.')
        {
            cleanedText = cleanedText.TrimStart('0');
            if (string.IsNullOrEmpty(cleanedText))
                cleanedText = "0";
        }

        // Update Entry text if it was cleaned
        if (cleanedText != newText)
        {
            // Prevent infinite loop by checking if text actually changed
            if (entry.Text != cleanedText)
            {
                entry.Text = cleanedText;
            }
            return; // TextChanged will fire again with cleaned text
        }

        // Handle empty or invalid input
        if (string.IsNullOrEmpty(cleanedText) || cleanedText == ".")
        {
            _viewModel.NewNoteGrade = 0;
            _viewModel.ValidateNewNoteGrade();
            return;
        }

        // Try to parse the cleaned text
        if (double.TryParse(cleanedText, System.Globalization.NumberStyles.Float, 
            System.Globalization.CultureInfo.InvariantCulture, out double grade))
        {
            // Apply system-specific constraints
            if (_viewModel.Settings?.GradingSystem?.StartsWith("Prozent") == true)
            {
                // Percentage: 0-100
                if (grade > 100)
                {
                    entry.Text = "100";
                    return;
                }
            }
            else if (_viewModel.Settings != null)
            {
                // Other systems: respect Min/Max (1-6 for Swiss/German)
                var max = Math.Max(_viewModel.Settings.MinGrade, _viewModel.Settings.MaxGrade);
                if (grade > max)
                {
                    entry.Text = max.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
                    return;
                }
            }

            // Valid grade - update ViewModel
            _viewModel.NewNoteGrade = grade;
            _viewModel.ValidateNewNoteGrade();
        }
        else
        {
            // Should never happen after cleaning, but just in case
            _viewModel.NewNoteGradeError = "Ungültige Eingabe";
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
}