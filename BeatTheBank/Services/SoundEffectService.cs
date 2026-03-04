using Plugin.Maui.Audio;

namespace BeatTheBank.Services;


[Singleton]
public class SoundEffectService
{
    readonly IAudioManager audioManager = AudioManager.Current;
    readonly Dictionary<string, IAudioPlayer> sounds = new();

    public void PlayAlarm() => this.Play("alarm.wav");
    public void PlayJackpot() => this.Play("jackpot.wav");

    public virtual void PlayBackgroundMusic()
    {
        var player = this.GetOrCreatePlayer("gamemusic.mp3");
        player.Loop = true;
        player.Volume = 0.25;
        player.Play();
    }

    public virtual void StopBackgroundMusic()
    {
        if (this.sounds.TryGetValue("gamemusic.mp3", out var player) && player.IsPlaying)
            player.Stop();
    }

    void Play(string fileName) => this.GetOrCreatePlayer(fileName).Play();

    IAudioPlayer GetOrCreatePlayer(string fileName)
    {
        if (!this.sounds.ContainsKey(fileName))
        {
            var player = this.audioManager.CreatePlayer(this.GetStream(fileName));
            this.sounds.Add(fileName, player);
        }
        return this.sounds[fileName];
    }


    Stream GetStream(string fileName)
    {
        #if IOS || ANDROID || MACCATALYST
        return FileSystem.OpenAppPackageFileAsync(fileName).GetAwaiter().GetResult();
        // #if IOS
        //         var fullPath = Path.Combine(Foundation.NSBundle.MainBundle.BundlePath, fileName);
        //         return File.OpenRead(fullPath);
        // #elif ANDROID
        //         var fullPath = Path.Combine("Assets", fileName);
        //         return Android.App.Application.Context.Assets!.Open(fullPath);
        // #elif MACCATALYST
        //         var fullPath = Path.Combine(Foundation.NSBundle.MainBundle.BundlePath, "Contents", "Resources", fileName);
        //         return File.OpenRead(fullPath);
#else
        return null;  //this.platform.AppContext.Assets!.Open(fileName);
#endif
    }
}