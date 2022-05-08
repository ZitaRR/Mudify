namespace Mudify.Shared.Entities;

public class Track
{
    public string Id { get; init; }
    public string Title { get; init; }
    public string Author { get; init; }
    public TimeSpan Duration { get; init; }
    public TimeSpan Position { get => position; init => position = value; }
    public byte[] Audio { get; init; }

    private TimeSpan position;

    public void SetPosition(double position)
    {
        this.position = TimeSpan.FromMilliseconds(position);
    }
}
