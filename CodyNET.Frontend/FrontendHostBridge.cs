using System;
using System.IO;
using System.Threading.Tasks;
using CodyNET.Common.Video;
using CodyNET.Core.Cody;
using CodyNET.Core.Devices;

namespace CodyNET.Frontend;

public static class FrontendHostBridge
{
    private static TaskCompletionSource<IScreenDevice> screenSource = CreateScreenSource();

    public static Task<IScreenDevice> ScreenTask => screenSource.Task;
    public static Keyboard? Keyboard { get; private set; }
    
    // Frontend Bindings
    private static Action<long>? setClockFrequencyAction;
    private static Action<FileInfo>? loadUart1SourceAction;
    private static Func<CpuRegisterSnapshot>? getRegisterSnapshotFunc;
    private static Func<CodyStatusSnapshot>? getStatusSnapshotFunc;

    public static void Reset()
    {
        screenSource = CreateScreenSource();
        Keyboard = null;

        setClockFrequencyAction = null;
        loadUart1SourceAction = null;
        getRegisterSnapshotFunc = null;
        getStatusSnapshotFunc = null;
    }

    public static void SetScreen(IScreenDevice screen)
    {
        screenSource.TrySetResult(screen);
    }

    public static void SetKeyboard(Keyboard keyboard)
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
    
    #region Frontend Bindings
    
    public static void RegisterUart1SourceLoader(Action<FileInfo> loader)
    {
        loadUart1SourceAction = loader;
    }

    public static void LoadUartSource(FileInfo fileInfo)
    {
        loadUart1SourceAction?.Invoke(fileInfo);
    }
    
    public static void RegisterClockFrequencySetter(Action<long> setter)
    {
        setClockFrequencyAction = setter;
    }
    
    public static void SetClockFrequency(long frequencyHz)
    {
        setClockFrequencyAction?.Invoke(frequencyHz);
    }

    public static void RegisterRegisterSnapshotProvider(Func<CpuRegisterSnapshot> provider)
    {
        getRegisterSnapshotFunc = provider;
    }
    
    public static void RegisterStatusSnapshotProvider(Func<CodyStatusSnapshot> provider)
    {
        getStatusSnapshotFunc = provider;
    }

    public static CpuRegisterSnapshot? GetRegisterSnapshot()
    {
        return getRegisterSnapshotFunc?.Invoke();
    }
    
    public static CodyStatusSnapshot? GetStatusSnapshot()
    {
        return getStatusSnapshotFunc?.Invoke();
    }
    
    #endregion
}
