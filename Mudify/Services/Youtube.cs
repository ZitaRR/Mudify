using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

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
}
