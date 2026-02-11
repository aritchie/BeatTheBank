using Plugin.Maui.Audio;

namespace BeatTheBank.Services;


public class SoundEffectService
{
    readonly IAudioManager audioManager = AudioManager.Current;
    readonly Dictionary<string, IAudioPlayer> sounds = new();

// #if ANDROID
//     readonly AndroidPlatform platform;
//     public SoundEffectService(AndroidPlatform platform) => this.platform = platform;
//
// #endif

    public void PlayAlarm() => this.Play("alarm.wav");
    public void PlayJackpot() => this.Play("jackpot.wav");

    void Play(string fileName)
    {
        //using var p = this.audioManager.CreatePlayer(this.GetStream(fileName));        
        //p.Play();
        if (!this.sounds.ContainsKey(fileName))
        {
            var player = this.audioManager.CreatePlayer(this.GetStream(fileName));
            this.sounds.Add(fileName, player);
        }
        this.sounds[fileName].Play();
    }


    Stream GetStream(string fileName)
    {
#if IOS
        var fullPath = Path.Combine(Foundation.NSBundle.MainBundle.BundlePath, fileName);
        return File.OpenRead(fullPath);
#elif MACCATALYST
        var fullPath = Path.Combine(Foundation.NSBundle.MainBundle.BundlePath, "Contents", "Resources", fileName);
        return File.OpenRead(fullPath);
#else
        return null;  //this.platform.AppContext.Assets!.Open(fileName);
#endif
    }
}