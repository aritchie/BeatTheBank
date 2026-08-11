using System.Collections.Concurrent;
using Shiny.Audio;

namespace BeatTheBank.Services;


[Singleton]
public class SoundEffectService(
    ILogger<SoundEffectService> logger,
    IAudioPlayer musicPlayer,
    IAudioPlayer effectsPlayer
)
{
    const string BackgroundMusicFile = "gamemusic.mp3";

    readonly ConcurrentDictionary<string, byte[]> cache = new();
    CancellationTokenSource? musicCancelSource;

    public virtual void PlayAlarm() => this.Play("alarm.wav");
    public virtual void PlayJackpot() => this.Play("jackpot.wav");

    public virtual void PlayBackgroundMusic()
    {
        if (this.musicCancelSource != null)
            return;

        var cancelSource = new CancellationTokenSource();
        this.musicCancelSource = cancelSource;
        var cancelToken = cancelSource.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                // IAudioPlayer has no loop flag - replay until we're told to stop
                while (!cancelToken.IsCancellationRequested)
                {
                    var data = await this.GetBytes(BackgroundMusicFile);
                    if (data == null)
                        return;

                    using var stream = new MemoryStream(data, false);
                    await musicPlayer.PlayAsync(stream, cancelToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to play background music");
            }
        });
    }

    public virtual void StopBackgroundMusic()
        => Interlocked.Exchange(ref this.musicCancelSource, null)?.Cancel();


    void Play(string fileName) => _ = Task.Run(async () =>
    {
        try
        {
            var data = await this.GetBytes(fileName);
            if (data == null)
                return;

            using var stream = new MemoryStream(data, false);
            await effectsPlayer.PlayAsync(stream);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to play {FileName}", fileName);
        }
    });


    async Task<byte[]?> GetBytes(string fileName)
    {
        if (this.cache.TryGetValue(fileName, out var cached))
            return cached;

#if IOS || ANDROID || MACCATALYST
        // the player consumes the stream, so keep the bytes around for replays
        await using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        var data = ms.ToArray();
        this.cache[fileName] = data;
        return data;
#else
        return await Task.FromResult<byte[]?>(null);
#endif
    }
}
