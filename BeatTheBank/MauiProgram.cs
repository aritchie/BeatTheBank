using System.Text.Json.Serialization;
using CommunityToolkit.Maui;
using Plugin.Maui.Audio;
using Shiny.SqliteDocumentDb;

namespace BeatTheBank;


[JsonSerializable(typeof(GameResult))]
internal partial class AppJsonContext : JsonSerializerContext;


public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder()
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .AddAudio()
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
        builder.Services.AddSqliteDocumentStore(opts =>
        {
            opts.ConnectionString = $"Data Source={Path.Combine(FileSystem.AppDataDirectory, "beatthebank.db3")}";
            opts.JsonSerializerOptions = AppJsonContext.Default.Options;
            opts.UseReflectionFallback = false;
        });
        builder.Services.AddGeneratedServices();

        return builder.Build();
    }
}
