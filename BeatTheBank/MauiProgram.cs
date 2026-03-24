using System.Text.Json.Serialization;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Plugin.Maui.Audio;
using Shiny.DocumentDb;
using Shiny.DocumentDb.Sqlite;

namespace BeatTheBank;


[JsonSerializable(typeof(GameResult))]
internal partial class AppJsonContext : JsonSerializerContext;


public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .AddAudio()
            .UseShinyShell(x => x.AddGeneratedMaps())
#if !DEBUG            
            .UseSentry(x => x.Dsn = builder.Configuration["SentryDsn"]!)
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Configuration.AddJsonStream(
            typeof(MauiProgram)
                .Assembly
                .GetManifestResourceStream("BeatTheBank.appsettings.json")!
        );
        builder.AddShinyMediator(x => x
            .AddMediatorRegistry()
            .UseMaui()
        );
        builder.Services.AddSingleton(DeviceDisplay.Current);
        builder.Services.AddDocumentStore(opts =>
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "beatthebank.db3");
            opts.DatabaseProvider = new SqliteDatabaseProvider($"Data Source={dbPath}");
            opts.JsonSerializerOptions = AppJsonContext.Default.Options;
            opts.UseReflectionFallback = false;
        });
        builder.Services.AddGeneratedServices();

        return builder.Build();
    }
}
