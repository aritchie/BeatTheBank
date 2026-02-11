using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;
using Plugin.Maui.Audio;

namespace BeatTheBank;


public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder()
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseShinyShell(x => x.AddGeneratedMaps())
#if !DEBUG
            .UseSentry(x => x.Dsn = AssemblyInfo.SentryDsn)
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.AddShinyMediator(x => x
            .AddMediatorRegistry()
            .UseMaui()
        );
        builder.Services.AddSingleton(DeviceDisplay.Current);
        builder.Services.AddGeneratedServices();

        return builder.Build();
    }
}
