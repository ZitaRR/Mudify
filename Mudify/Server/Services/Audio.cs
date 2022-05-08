using Mudify.Shared.Entities;
using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace Mudify.Server.Services;

public class Audio
{
    private const int BATCH_SIZE = 30;

    private readonly YoutubeClient client;

    public Audio()
    {
        client = new();
    }

    public async IAsyncEnumerable<Track> SearchVideosAsync(string query)
    {
        IAsyncEnumerable<VideoSearchResult> results = client.Search.GetVideosAsync(query);
        int index = 0;

        await foreach (VideoSearchResult result in results)
        {
            if (index++ >= BATCH_SIZE)
            {
                yield break;
            }
            yield return new()
            {
                Id = result.Id,
                Title = result.Title,
                Author = result.Author.ChannelTitle,
                Duration = result.Duration ?? default,
                Position = TimeSpan.Zero
            };
        }
    }

    public async Task<byte[]> GetAudioAsync(Track track)
    {
        if (track is null)
        {
            throw new ArgumentNullException(nameof(track));
        }

        StreamManifest manifest = await client.Videos.Streams.GetManifestAsync(track.Id);
        IStreamInfo info = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
        Stream stream = await client.Videos.Streams.GetAsync(info);

        MemoryStream memory = new();
        await stream.CopyToAsync(memory);

        return memory.ToArray();
    }
}
