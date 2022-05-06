using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Mudify.Shared.Entities;

namespace Mudify.Client.Services;

public partial class Audio : IAsyncDisposable
{
    private static Func<double, Task> onStart;
    private static Func<long, Task> onProgress;
    private static Func<Task> onFinish;

    public Func<Track, Task> OnStart { get; set; }
    public Func<Track, Task> OnProgress { get; set; }
    public Func<Track, Task> OnFinish { get; set; }
    public Func<List<Track>, Task> OnTracksUpdated { get; set; }
    public Track Current { get; set; }
    public List<Track> Tracks = new();
    public bool IsPaused { get; set; } = true;

    private readonly IJSRuntime js;
    private readonly HubConnection hub;
    private readonly NavigationManager navigation;
    private Track temp;

    public Audio(IJSRuntime js, NavigationManager navigation)
    {
        this.js = js;
        this.navigation = navigation;

        onStart += TrackStart;
        onProgress += TrackProgress;
        onFinish += TrackFinish;

        hub = new HubConnectionBuilder()
            .WithUrl(this.navigation.ToAbsoluteUri("/audio"))
            .Build();

        hub.On<byte[]>("ReceiveAudio", async (audio) =>
        {
            IsPaused = false;
            await js.InvokeVoidAsync("setupAudio");
            await js.InvokeVoidAsync("play", audio);
        });

        hub.On<Track>("ReceiveTrack", async (track) =>
        {
            Tracks.Add(track);
            await OnTracksUpdated?.Invoke(Tracks);
        });

        hub.StartAsync();
    }

    public async Task PlayAsync(Track track)
    {
        if (track is null)
        {
            throw new ArgumentNullException(nameof(track));
        }

        temp = track;
        await hub.SendAsync("GetAudio", track);
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

    public async Task SearchAsync(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            throw new ArgumentNullException(nameof(query));
        }

        Tracks.Clear();
        await hub.SendAsync("GetTracks", query);
    }

    private async Task TrackStart(double duration)
    {
        Current = new()
        {
            Title = temp.Title,
            Author = temp.Author,
            Duration = TimeSpan.FromMilliseconds(duration)
        };

        await OnStart?.Invoke(Current);
    }

    private async Task TrackProgress(long position)
    {
        Current.SetPosition(position);
        await OnProgress?.Invoke(Current);
    }

    private async Task TrackFinish()
    {
        Track clone = new()
        {
            Title = Current.Title,
            Author = Current.Author,
            Duration = Current.Duration,
            Position = Current.Duration
        };


        await OnFinish?.Invoke(clone);
    }

    public async ValueTask DisposeAsync()
    {
        if (hub is null)
        {
            return;
        }

        await hub.DisposeAsync();
    }
}
