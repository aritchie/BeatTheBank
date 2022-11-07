using CommunityToolkit.Maui;
using Plugin.Maui.Audio;
using Prism.DryIoc;

namespace BeatTheBank;


public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder()
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseShinyFramework(
                new DryIocContainerExtension(),
                prism => prism.OnAppStart("NavigationPage/MainPage")
            )
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold"); 
            });

        RegisterServices(builder);
        RegisterViews(builder.Services);

        return builder.Build();
    }


    static void RegisterServices(MauiAppBuilder builder)
    {
        var s = builder.Services;

        s.AddSingleton(AudioManager.Current);
        s.AddSingleton(TextToSpeech.Default);
        s.AddSingleton(DeviceDisplay.Current);
        s.AddSpeechRecognition();
        s.AddSingleton<SoundEffectService>();

        s.AddGlobalCommandExceptionHandler(new(
#if DEBUG
            ErrorAlertType.FullError
#else
            ErrorAlertType.NoLocalize
#endif
        ));
    }


    static void RegisterViews(IServiceCollection s)
    {
        s.RegisterForNavigation<MainPage, MainViewModel>();
    }
}
