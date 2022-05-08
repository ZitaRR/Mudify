using Microsoft.JSInterop;

namespace Mudify.Client.Services;

public partial class Audio
{
    [JSInvokable]
    public static async Task OnTrackStart(double duration)
    {
        if (onStart is null)
        {
            return;
        }

        await onStart.Invoke(duration);
    }

    [JSInvokable]
    public static async Task OnTrackProgress(double position, bool trackEnded)
    {
        if (onProgress is null)
        {
            return;
        }

        await onProgress.Invoke(position);

        if (trackEnded)
        {
            await OnTrackFinish();
        }
    }

    [JSInvokable]
    public static async Task OnTrackFinish()
    {
        if (onFinish is null)
        {
            return;
        }

        await onFinish.Invoke();
    }
}
