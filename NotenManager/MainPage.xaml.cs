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
}