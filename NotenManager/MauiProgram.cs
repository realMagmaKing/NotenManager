using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace NotenManager;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
     
        try
        {
 builder
       .UseMauiApp<App>()
      .UseSkiaSharp()
          .ConfigureFonts(fonts =>
            {
  fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
     fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
  });

#if DEBUG
            builder.Logging.AddDebug();
       builder.Logging.SetMinimumLevel(LogLevel.Trace);
#endif

      return builder.Build();
        }
  catch (Exception ex)
      {
    System.Diagnostics.Debug.WriteLine($"MauiProgram Error: {ex.Message}");
        System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
       throw;
        }
    }
}