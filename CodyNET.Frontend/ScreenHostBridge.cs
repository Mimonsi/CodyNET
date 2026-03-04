using System;
using System.Threading.Tasks;
using CodyNET.Common.Video;
using CodyNET.Core.Devices;

namespace CodyNET.Frontend;

public static class ScreenHostBridge
{
    private static TaskCompletionSource<IScreenDevice> screenSource = CreateScreenSource();

    public static Task<IScreenDevice> ScreenTask => screenSource.Task;
    public static LogicalKeyboard? Keyboard { get; private set; }

    public static void Reset()
    {
        screenSource = CreateScreenSource();
        Keyboard = null;
    }

    public static void SetScreen(IScreenDevice screen)
    {
        screenSource.TrySetResult(screen);
    }

    public static void SetKeyboard(LogicalKeyboard keyboard)
    {
        Keyboard = keyboard;
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