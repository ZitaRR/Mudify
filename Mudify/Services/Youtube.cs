using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Mudify.Services;

public class Youtube
{
    private const int RESULT_SIZE = 35;

    private readonly YoutubeClient client;

    public Youtube()
    {
        client = new();
    }

    public async IAsyncEnumerable<VideoSearchResult> SearchVideosAsync(string query)
    {
        IAsyncEnumerable<VideoSearchResult> results = client.Search.GetVideosAsync(query);
        int index = 0;
        await foreach (VideoSearchResult video in results)
        {
            if (index++ >= RESULT_SIZE)
            {
                yield break;
            }
            yield return video;
        }
    }

    public async Task<string> GetAudioBase64String(VideoSearchResult video)
    {
        if (video is null)
        {
            throw new ArgumentNullException(nameof(video));
        }

        StreamManifest manifest = await client.Videos.Streams.GetManifestAsync(video.Id);
        IStreamInfo info = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
        Stream stream = await client.Videos.Streams.GetAsync(info);

        MemoryStream memory = new();
        await stream.CopyToAsync(memory);

        return $"data:audio/{info.Container.Name};base64,{Convert.ToBase64String(memory.ToArray())}";
    }
}
