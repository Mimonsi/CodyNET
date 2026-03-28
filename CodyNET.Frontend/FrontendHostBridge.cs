using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CodyNET.Common.Video;
using CodyNET.Core.Cody;
using CodyNET.Core.Devices;
using Debugger = CodyNET.Core.Devices.Debugger;

namespace CodyNET.Frontend;

public static class FrontendHostBridge
{
    private static TaskCompletionSource<IScreenDevice> screenSource = CreateScreenSource();

    public static Task<IScreenDevice> ScreenTask => screenSource.Task;
    public static Keyboard? Keyboard { get; private set; }
    public static Debugger? Debugger { get; private set; }
    
    public static long InitialClockFrequency { get; private set; }

    // Frontend Bindings
    private static Action<long>? setClockFrequencyAction; // Buttons to set frequency
    private static Action<FileInfo>? loadUart1SourceAction; // Button for loading UART1 source data from file
    private static Func<CpuRegisterSnapshot>? getRegisterSnapshotFunc; // Display for Register and CPU Flags
    private static Func<CodyStatusSnapshot>? getStatusSnapshotFunc; // Display for Frequency and FPS
    private static Action<int>? setRunStateAction; // Buttons for Pause, Resume and Single Step
    private static Action? resetEmulatorAction; // Button for resetting the emulator
    private static Action<FileInfo>? saveSnapshotAction;
    private static Action<FileInfo>? loadSnapshotAction;

    public static void Reset()
    {
        screenSource = CreateScreenSource();
        Keyboard = null;

        setClockFrequencyAction = null;
        loadUart1SourceAction = null;
        getRegisterSnapshotFunc = null;
        getStatusSnapshotFunc = null;
        setRunStateAction = null;
        resetEmulatorAction = null;
        saveSnapshotAction = null;
        loadSnapshotAction = null;
    }

    public static void SetScreen(IScreenDevice screen)
    {
        screenSource.TrySetResult(screen);
    }

    public static void SetKeyboard(Keyboard keyboard)
    {
        Keyboard = keyboard;
    }
    
    public static void SetDebugger(Debugger debugger)
    {
        Debugger = debugger;
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
    
    public static void RegisterClockFrequencySetter(Action<long> setter, long initialFrequencyHz)
    {
        setClockFrequencyAction = setter;
        InitialClockFrequency = initialFrequencyHz;
    }
    
    public static void SetClockFrequency(long frequencyHz)
    {
        setClockFrequencyAction?.Invoke(frequencyHz);
    }
    
    public static void RegisterRunStateAction(Action<int> setter)
    {
        setRunStateAction = setter;
    }

    public static void SetRunState(int runState)
    {
        setRunStateAction?.Invoke(runState);
    }

    public static void RegisterResetAction(Action action)
    {
        resetEmulatorAction = action;
    }

    public static void ResetEmulator()
    {
        resetEmulatorAction?.Invoke();
    }

    public static void RegisterSaveSnapshotAction(Action<FileInfo> action)
    {
        saveSnapshotAction = action;
    }

    public static void SaveSnapshot(FileInfo file)
    {
        saveSnapshotAction?.Invoke(file);
    }

    public static void RegisterLoadSnapshotAction(Action<FileInfo> action)
    {
        loadSnapshotAction = action;
    }

    public static void LoadSnapshot(FileInfo file)
    {
        loadSnapshotAction?.Invoke(file);
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
