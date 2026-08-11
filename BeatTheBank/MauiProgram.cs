using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Shiny.Audio;
using Shiny.Speech;
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
        builder.Configuration.AddJsonStream(
            typeof(MauiProgram)
                .Assembly
                .GetManifestResourceStream("BeatTheBank.appsettings.json")!
        );
        
        builder
            .UseMauiApp<App>()
            .AddShinyMediator(x => x
                .AddMediatorRegistry()
                .UseMaui()
            )
            .UseShinyShell(x => x
                .AddGeneratedMaps()
                .UseUxDiversDialogs()
            )
#if !DEBUG            
            .UseSentry(x => x.Dsn = builder.Configuration["SentryDsn"]!)
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton(DeviceDisplay.Current);
        builder.Services.AddDocumentStore(opts =>
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "beatthebank.db3");
            opts.DatabaseProvider = new SqliteDatabaseProvider($"Data Source={dbPath}");
            opts.JsonSerializerOptions = AppJsonContext.Default.Options;
            opts.UseReflectionFallback = false;
        });
        // Shiny.Audio registers IAudioPlayer as a singleton and a player only holds one
        // stream at a time - go transient so background music & sound effects can overlap
#if IOS
        builder.Services.AddTransient<IAudioPlayer, AppleAudioPlayer>();
#elif ANDROID
        builder.Services.AddTransient<IAudioPlayer, AndroidAudioPlayer>();
#endif
        builder.Services.AddSpeechServices();
        builder.Services.AddGeneratedServices();

        return builder.Build();
    }
}
