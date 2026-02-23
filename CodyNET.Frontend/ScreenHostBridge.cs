using System;
using System.Threading.Tasks;
using CodyNET.Common.Video;

namespace CodyNET.Frontend;

public static class ScreenHostBridge
{
    private static TaskCompletionSource<IScreenDevice> screenSource = CreateScreenSource();

    public static Task<IScreenDevice> ScreenTask => screenSource.Task;

    public static void Reset()
    {
        screenSource = CreateScreenSource();
    }

    public static void SetScreen(IScreenDevice screen)
    {
        screenSource.TrySetResult(screen);
    }

    public static void SetInitializationError(Exception exception)
    {
        screenSource.TrySetException(exception);
    }

    private static TaskCompletionSource<IScreenDevice> CreateScreenSource()
    {
        return new TaskCompletionSource<IScreenDevice>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
