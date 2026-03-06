using System;
using System.Threading.Tasks;
using CodyNET.Common.Video;
using CodyNET.Core.Devices;

namespace CodyNET.Frontend;

public static class FrontendHostBridge
{
    private static TaskCompletionSource<IScreenDevice> screenSource = CreateScreenSource();

    public static Task<IScreenDevice> ScreenTask => screenSource.Task;
    public static Keyboard? Keyboard { get; private set; }
    
    // Frontend Bindings
    private static Action<long>? setClockFrequencyAction;

    public static void Reset()
    {
        screenSource = CreateScreenSource();
        Keyboard = null;

        setClockFrequencyAction = null;
    }

    public static void SetScreen(IScreenDevice screen)
    {
        screenSource.TrySetResult(screen);
    }

    public static void SetKeyboard(Keyboard keyboard)
    {
        Keyboard = keyboard;
    }
    
    public static void SetClockFrequency(long frequencyHz)
    {
        setClockFrequencyAction?.Invoke(frequencyHz);
    }

    public static void RegisterClockFrequencySetter(Action<long> setter)
    {
        setClockFrequencyAction = setter;
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