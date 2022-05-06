using Microsoft.AspNetCore.SignalR;
using Mudify.Server.Services;
using Mudify.Shared.Entities;

namespace Mudify.Server.Hubs;

public class AudioHub : Hub
{
    private readonly Audio audio;

    public AudioHub(Audio audio)
    {
        this.audio = audio;
    }

    public async Task GetTracks(string query)
    {
        await foreach(Track track in audio.SearchVideosAsync(query))
        {
            await Clients.Caller.SendAsync("ReceiveTrack", track);
        }
    }

    public async Task GetAudio(Track track)
    {
        byte[] bytes = await audio.GetAudioAsync(track);
        await Clients.Caller.SendAsync("ReceiveAudio", bytes);
    }

    public async Task Assem(string assem)
    {
        Console.WriteLine(assem);
    }
}
