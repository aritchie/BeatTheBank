using System.Globalization;
using CommunityToolkit.Maui.Media;

namespace BeatTheBank.Services;

public interface ISpeechService
{
    bool IsListening { get; }
    Task<bool> StartListening(Action<string> onSpeechDetected);
    Task StopListening();

    Task Speak(string text);
    Task SpeakIterations(int pauseBetween, params IEnumerable<string> sentences);
}

[Singleton]
public class SpeechService(ILogger<SpeechService> logger) : ISpeechService
{
    readonly ITextToSpeech Tts = TextToSpeech.Default;
    readonly ISpeechToText Stt = SpeechToText.Default;


    public Task Speak(string text) => Tts.SpeakAsync(text);

    public async Task SpeakIterations(int pauseBetween, params IEnumerable<string> sentences)
    {
        foreach (var s in sentences)
        {
            await this.Speak(s);
            await Task.Delay(pauseBetween);
        }
    }


    public bool IsListening => this.Stt.CurrentState == SpeechToTextState.Listening;

    Action<string>? listenCallback;
    public async Task<bool> StartListening(Action<string> onSpeechDetected)
    {
        if (Stt.CurrentState == SpeechToTextState.Listening)
            throw new InvalidOperationException("Listening is already in progress.");

        this.listenCallback = onSpeechDetected;
        var granted = await Stt.RequestPermissions();
        if (!granted)
            return false;

        Stt.RecognitionResultCompleted += this.OnRecognitionResultCompleted;
        await Stt.StartListenAsync(new SpeechToTextOptions
        {
            Culture = new CultureInfo("en-US"),
            ShouldReportPartialResults = false
        });
        return true;
    }

    
    public Task StopListening()
    {
        this.listenCallback = null;
        if (Stt.CurrentState == SpeechToTextState.Listening)
        {
            return Stt.StopListenAsync();
        }

        return Task.CompletedTask;
    }

    void OnRecognitionResultCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
    {
        logger.LogInformation("Incoming Speech Result");
        if (!e.RecognitionResult.IsSuccessful)
            return;

        var txt = e.RecognitionResult.Text?.ToLower() ?? String.Empty;
        logger.LogInformation("Speech Result: {txt}", txt);
        
        this.listenCallback?.Invoke(txt);
    }
}