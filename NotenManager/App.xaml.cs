namespace NotenManager;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Add global exception handler
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        MainPage = new MainPage();
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        System.Diagnostics.Debug.WriteLine($"Unhandled Exception: {exception?.Message}");
        System.Diagnostics.Debug.WriteLine($"Stack Trace: {exception?.StackTrace}");
    }

    private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Unobserved Task Exception: {e.Exception?.Message}");
        e.SetObserved();
    }
}