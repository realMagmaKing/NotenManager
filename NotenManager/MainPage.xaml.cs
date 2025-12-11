using NotenManager.ViewModels;
using System.ComponentModel;
using Microsoft.Maui.ApplicationModel;
using Microcharts;
using Microcharts.Maui;
using Microsoft.Maui.Controls;

namespace NotenManager;

public partial class MainPage : ContentPage
{
    private MainViewModel _viewModel;

    public MainPage()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        BindingContext = _viewModel;

        // Subscribe to property changes so we can update the native ChartView to avoid overlays
        if (_viewModel is INotifyPropertyChanged pc)
        {
            pc.PropertyChanged += ViewModel_PropertyChanged;
        }

        // Dark Mode initial setzen
        Application.Current.UserAppTheme = _viewModel.IsDarkMode ? AppTheme.Dark : AppTheme.Light;
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // When the GradeChart object changes, replace the ChartView to avoid drawing overlays
        if (e.PropertyName == nameof(MainViewModel.GradeChart))
        {
            ReplaceChart(_viewModel.GradeChart);
        }

        // When navigating away from Notes page, ensure chart is cleared
        if (e.PropertyName == nameof(MainViewModel.CurrentPage))
        {
            if (_viewModel.CurrentPage != "Notes")
            {
                ReplaceChart(null);
            }
        }
    }

    private void ReplaceChart(Chart chart)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                // Find parent layout that currently contains GradeChartView
                var parent = GradeChartView?.Parent as Microsoft.Maui.Controls.Layout;
                if (parent == null)
                {
                    // fallback: try visual tree search
                    return;
                }

                var index = parent.Children.IndexOf(GradeChartView);
                if (index < 0) index = parent.Children.Count;

                // Remove old instance
                parent.Children.Remove(GradeChartView);

                // Create new ChartView instance
                var newChartView = new Microcharts.Maui.ChartView
                {
                    Chart = chart,
                    HeightRequest = 280,
                    IsVisible = chart != null
                };

                // Insert new instance at same position
                parent.Children.Insert(Math.Min(index, parent.Children.Count), newChartView);

                // Update generated field reference so XAML bindings still work
                try
                {
                    GradeChartView = newChartView;
                }
                catch
                {
                    // ignore if field inaccessible
                }
            }
            catch
            {
                // silent
            }
        });
    }

    private void OnDarkModeToggled(object sender, ToggledEventArgs e)
    {
        Application.Current.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
        _viewModel.Settings.IsDarkMode = e.Value;
    }
}