using Microsoft.JSInterop;
using Mudify.Core.Entities;
using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Mudify.Core.Services;

public class Audio
{
    public static Func<Track, Task> OnStart;
    public static Func<Track, Task> OnFinished;
    public static Func<Track, Task> OnProgress;

    public static Track Current { get; private set; }
    public static DateTime CurrentStart { get; private set; }

    private readonly IJSRuntime js;
    private readonly YoutubeClient client;

    public Audio(IJSRuntime js)
    {
        this.js = js;
        client = new();
    }

    public async Task PlayAsync(VideoSearchResult video)
    {
        if (video.Duration is null)
        {
            throw new ArgumentNullException(nameof(video));
        }

        CurrentStart = DateTime.Now;

        StreamManifest manifest = await client.Videos.Streams.GetManifestAsync(video.Id);
        IStreamInfo info = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
        Stream stream = await client.Videos.Streams.GetAsync(info);

        MemoryStream memory = new();
        await stream.CopyToAsync(memory);

        Current = new()
        {
            Title = video.Title,
            Author = video.Author.ChannelTitle,
            Duration = video.Duration ?? default,
            Audio = memory.ToArray()
        };

        await js.InvokeVoidAsync("buffer", Current);
    }

    [JSInvokable]
    public static async Task OnTrackStart()
    {
        CurrentStart = DateTime.Now;
        await OnStart?.Invoke(Current)!;
    }

    [JSInvokable]
    public static async Task OnTrackEnd()
    {
        Track clone = new()
        {
            Title = Current.Title,
            Author = Current.Author,
            Duration = Current.Duration,
            Audio = Current.Audio
        };
        Current = null!;

        await OnFinished?.Invoke(clone)!;
    }

    [JSInvokable]
    public static async Task OnTrackProgress()
    {
        await OnProgress?.Invoke(Current);
    }
}
