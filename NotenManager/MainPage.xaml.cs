using NotenManager.ViewModels;

namespace NotenManager;

public partial class MainPage : ContentPage
{
    private MainViewModel _viewModel;

    public MainPage()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        BindingContext = _viewModel;

        // Dark Mode initial setzen
        Application.Current.UserAppTheme = _viewModel.IsDarkMode ? AppTheme.Dark : AppTheme.Light;
    }

    private void OnDarkModeToggled(object sender, ToggledEventArgs e)
    {
        Application.Current.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
        _viewModel.Settings.IsDarkMode = e.Value;
    }
}