using Microsoft.JSInterop;
using Mudify.Core.Entities;
using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace Mudify.Core.Services;

public class Audio
{
    private static Func<Task> onStart;
    private static Func<Task> onFinished;
    private static Func<long, Task> onProgress;

    public Func<Track, Task> OnStart { get; set; }
    public Func<Track, Task> OnFinished { get; set; }
    public Func<Track, Task> OnProgress { get; set; }
    public Track Current { get; private set; }
    public bool IsPaused { get; private set; } = true;

    private readonly IJSRuntime js;
    private readonly YoutubeClient client;

    public Audio(IJSRuntime js)
    {
        this.js = js;
        client = new();

        onStart += StartTrack;
        onFinished += FinishedTrack;
        onProgress += ProgressTrack;
    }

    private async Task StartTrack()
    {
        await OnStart?.Invoke(Current);
    }

    private async Task FinishedTrack()
    {
        Track clone = new()
        {
            Title = Current.Title,
            Author = Current.Author,
            Duration = Current.Duration,
            Position = Current.Duration,
            Audio = Current.Audio
        };
        Current = null;

        await OnFinished?.Invoke(Current);
    }

    private async Task ProgressTrack(long position)
    {
        Current.SetPosition(position);
        await OnProgress?.Invoke(Current);
    }

    public async Task PlayAsync(VideoSearchResult video)
    {
        if (video.Duration is null)
        {
            throw new ArgumentNullException(nameof(video));
        }

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

        IsPaused = false;
        await js.InvokeVoidAsync("play", Current);
    }

    public async Task PauseAsync()
    {
        if (Current is null)
        {
            return;
        }

        IsPaused = true;
        await js.InvokeVoidAsync("pause");
    }

    public async Task ResumeAsync()
    {
        if (Current is null)
        {
            return;
        }

        IsPaused = false;
        await js.InvokeVoidAsync("unpause");
    }

    [JSInvokable]
    public static async Task OnTrackStart()
    {
        await onStart?.Invoke();
    }

    [JSInvokable]
    public static async Task OnTrackEnd()
    {
        await onFinished?.Invoke();
    }

    [JSInvokable]
    public static async Task OnTrackProgress(long position)
    {
        await onProgress?.Invoke(position);
    }
}
