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
    public static async Task OnTrackProgress(long position)
    {
        if (onProgress is null)
        {
            return;
        }

        await onProgress.Invoke(position);
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
