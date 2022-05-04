namespace Mudify.Core.Entities;

public class Track
{
    public string Title { get;  init; }
    public string Author { get; init; }
    public TimeSpan Duration { get; init; }
    public TimeSpan Position => DateTime.Now - Services.Audio.CurrentStart;
    public byte[] Audio { get; init; }
}
