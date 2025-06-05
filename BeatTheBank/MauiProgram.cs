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
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold"); 
            });

        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton(TextToSpeech.Default);
        builder.Services.AddSingleton(SpeechToText.Default);
        builder.Services.AddSingleton(DeviceDisplay.Current);
        builder.Services.AddSingleton<SoundEffectService>();

        return builder.Build();
    }
}
