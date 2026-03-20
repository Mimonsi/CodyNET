using System.Diagnostics;
using CodyNET.Assembler;
using CodyNET.Common;
using CodyNET.Common.Video;
using CodyNET.Common.Utils;
using CodyNET.Core.Devices;
using CodyNET.Core.Interfaces;
using CodyNET.Core.Roms;
using Debugger = CodyNET.Core.Devices.Debugger;

namespace CodyNET.Core.Cody;

// Options when creating a new cody, filled with default values for boot command
public sealed record CodySetupOptions
{
    public static CodySetupOptions Default => new();
    
    public long FrequencyHz { get; init; } = 1_000_000; // 1 MHz, -1 = as fast as possible
    public bool EnableDebugger { get; init; } = true;
    public bool PhysicalKeyboard { get; init; } = true; // Use physical keyboard input (instead of logical)
    public bool EnableScreen { get; init; } = true;
    public bool EnableProfiler { get; init; } = true;
}

public sealed record CodyLoadOptions
{
    public FileInfo? File { get; init; }
    public bool AsCartridge { get; init; }
    public ushort LoadAddress { get; init; } = 0xE000;
    public bool AutoSetResetVector { get; set; } = false; // Only used when cartridge is loaded, set by program
    public ushort? ResetVectorOverride { get; init; }
    public ushort? IrqVectorOverride { get; init; }
    public ushort? NmiVectorOverride { get; init; }
    public FileInfo? Uart1Source { get; init; }
    public FileInfo? Uart2Source { get; init; }
    public bool FixUartNewlines { get; set; }
}

public class Cody
{
    public Cpu Cpu;
    public Memory Memory => Cpu.Memory;
    public Debugger? Debugger { get; private set; }
    public IVideoDevice? VID { get; private set; }
    public VersatileInterfaceAdapter? VIA { get; init; }
    public Keyboard? Keyboard { get; private set; }
    public UartDevice Uart1 { get; private set; }
    public UartDevice Uart2 { get; private set; }
    public IScreenDevice? Screen { get; init; }
    public Profiler? Profiler;

    public long FrequencyHz
    {
        get => Cpu.CyclesPerSecond;
        set
        {
            Cpu.CyclesPerSecond = value;
        }
    }

    public bool FastMode
    {
        get => Cpu.CyclesPerSecond == -1;
        set => FrequencyHz = value ? -1 : 1_000_000;
    }

    // TODO: Cody shouldn't need setup options, as Factory create devices based on options and passes them in. Refactor to remove this dependency.
    public Cody(CodySetupOptions options, IVideoDevice? videoDevice = null, IScreenDevice? screen = null)
    {
        VID = videoDevice;
        Screen = screen;
        
        // 1. Set up CPU (and memory)
        // CPU also initializes memory
        Cpu = new Cpu
        {
            CyclesPerSecond = options.FrequencyHz
        };

        // 2. Set up devices
        if (options.EnableProfiler)
        {
            Profiler = new Profiler(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)); // 5 second window for averaging
        }
        if (options.EnableDebugger)
        {
            // watch addresses 0xA000 - 0xA3E7 (text screen)
            /*var addresses = new List<ushort>();
            for (ushort addr = 0xA000; addr <= 0xA3E7; addr++)
                addresses.Add(addr);*/
            Debugger = new Debugger(Cpu)
            {
                WatchAddresses = [],
                Breakpoints = [] //[0xFD93]
            };
            Memory.RegisterDevice(Debugger);
            Memory.RegisterTap(Debugger);
        }

        // Input Setup (VIA + Keyboard)
        KeyState keyState = new KeyState();
        VIA = new VersatileInterfaceAdapter(keyState);
        Memory.RegisterDevice(VIA);
        Keyboard = new Keyboard(keyState)
        {
            UsePhysicalKeyboard = options.PhysicalKeyboard
        };
        
        // Uart Setup: Uart1 is prop plug, Uart2 is extension port (cartridge)
        Uart1 = new UartDevice(0xD480);
        Memory.RegisterDevice(Uart1);
        Uart2 = new UartDevice(0xD4A0);
        Memory.RegisterDevice(Uart2);

        if (VID != null)
        {
            Memory.RegisterDevice(VID);
        }
    }

    public void Reset()
    {
        Cpu.Reset();
    }

    public StepResult Step()
    {
        return Cpu.Step();
    }

    private Stopwatch screenStopwatch = new();
    public void RunUntilFinish()
    {
        screenStopwatch.Start();
        StepResult result;
        //Log.Level = LogLevel.Trace;
        do
        {
            result = Cpu.Step();
            Profiler?.SampleCpu(Cpu.TotalCyclesExecuted, Cpu.CyclesPerSecond);
            // TEMP
            if (Debugger is not null && Debugger.IsAtBreakpoint())
            {
                Log.Info("Execution paused by debugger on PC={PC:X4}", Cpu.PC);
                //break;
            }

            // 60 times per second
            if (screenStopwatch.Elapsed >= TimeSpan.FromSeconds(1.0 / 60))
            {
                screenStopwatch.Restart();
                if (VID != null)
                {
                    var frame = VID.RenderTextFrame(Cpu.Memory);
                    Screen?.RenderFrame(frame);
                    Profiler?.FrameRendered();
                }
            }
        } while (result != StepResult.Stopped);
    }

    /// <summary>
    /// Performance testing helper. Returns cycles spent in one step.
    /// </summary>
    public long SingleStep()
    {
        Step();
        long cycles = Cpu.TotalCyclesExecuted;
        Cpu.TotalCyclesExecuted = 0;
        return cycles;
    }
    
    private void CheckFilePath(FileInfo? file)
    {
        if (file is null)
            throw new ArgumentNullException(nameof(file), "File path must be provided.");
        if (!file.Exists)
            throw new FileNotFoundException($"File not found: {file.FullName}");
    }

    /// <summary>
    /// Loads the given binary file as uart source for the specified UART (1 = prop plug, 2 = cartridge)
    /// </summary>
    /// <param name="file"></param>
    /// <param name="uartNumber"></param>
    /// <param name="fixNewlines"></param>
    public void LoadUartSource(FileInfo file, int uartNumber = 1, bool fixNewlines = false)
    {
        if (file.Extension == ".bas")
        {
            fixNewlines = true; // Automatically set to true when reading basic file. TODO: Find better solution
            Log.Info("BASIC file being set as UART source, enabling fix-newlines parameter");
        }
        var source = UartSource.FromFile(file, fixNewlines);

        if (uartNumber == 1)
            Uart1.Source = source;
        else if (uartNumber == 2)
            Uart2.Source = source;
        else
            throw new ArgumentException("Invalid UART number. Must be 1 or 2.", nameof(uartNumber));
    }

    public void LoadBinaryFile(CodyLoadOptions options)
    {
        CheckFilePath(options.File);

        var bytes = File.ReadAllBytes(options.File!.FullName);
        LoadBinary(bytes, options);
    }
    
    public void LoadBinary(byte[] bytes, CodyLoadOptions? options = null)
    {
        if (bytes is null) throw new ArgumentNullException(nameof(bytes));
        
        options ??= new CodyLoadOptions();

        if (options.Uart1Source != null) // TODO: Check if this is the right place to load UART source
            LoadUartSource(options.Uart1Source, 1, options.FixUartNewlines);
        if (options.Uart2Source != null)
            LoadUartSource(options.Uart2Source, 2, options.FixUartNewlines);
        // Uart2 load here
        var (image, loadAddress) = ParseImage(bytes, options);
        LoadImage(image, loadAddress);
        ApplyVectors(loadAddress, options);
    }

    public void LoadAssemblyFile(CodyLoadOptions loadOptions)
    {
        CheckFilePath(loadOptions.File);

        var program = TassAssembler.AssembleFile(loadOptions.File!.FullName);
        LoadBinary(program, loadOptions);
    }
    
    public void RunBinaryFile(CodyLoadOptions loadOptions)
    {
        CheckFilePath(loadOptions.File);
        Log.Debug("Running binary file: '{fileName}'", loadOptions.File!.FullName);

        var bytes = File.ReadAllBytes(loadOptions.File!.FullName);
        RunBinary(bytes, loadOptions);
    }
    
    public void RunBinary(byte[] bytes, CodyLoadOptions loadOptions)
    {
        LoadBinary(bytes, loadOptions);
        Reset();
        RunUntilFinish();
    }

    public void RunAssemblyFile(CodyLoadOptions loadOptions)
    {
        CheckFilePath(loadOptions.File);
        
        LoadAssemblyFile(loadOptions);
        Reset();
        RunUntilFinish();
    }
    
    /// <summary>
    /// Boots the machine by loading the built-in CodyBASIC ROM.
    /// </summary>
    public void Boot(CodyLoadOptions? options = null)
    {
        Log.Debug("Booting Cody with CodyBASIC ROM.");
        try
        {
            options ??= new CodyLoadOptions();
            options = options with
            {
                LoadAddress = 0xE000,
                AsCartridge = false,
                AutoSetResetVector = false,
            };
            var codyBasicRom = RomLoader.LoadEmbeddedCodyBasic();
            RunBinary(codyBasicRom, options);
        }
        catch (FileNotFoundException e)
        {
            Log.Error("Bundled CodyBASIC ROM not found as embedded resource. If this error keeps happening, please run and specify file manually instead. {message}", e.Message);
        }
    }

    public void LoadImage(byte[] data, ushort loadAddress)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));
        
        Memory.LoadBytes(data, loadAddress);
    }

    private static (byte[] image, ushort loadAddress) ParseImage(byte[] bytes, CodyLoadOptions options)
    {
        if (!options.AsCartridge)
            return (bytes, options.LoadAddress);

        if (bytes.Length < 4)
            throw new InvalidDataException("Cartridge header must be at least 4 bytes.");

        ushort start = (ushort)(bytes[0] | (bytes[1] << 8));
        ushort end = (ushort)(bytes[2] | (bytes[3] << 8));

        int len = end - start + 1;
        if (len <= 0)
            throw new InvalidDataException("Cartridge header invalid (end < start).");

        if (bytes.Length < 4 + len)
            throw new InvalidDataException("Cartridge image truncated (data shorter than header claims).");

        var image = new byte[len];
        Buffer.BlockCopy(bytes, 4, image, 0, len);
        options.AutoSetResetVector = true; // Auto set reset vector to start of cartridge if not explicitly overridden
        return (image, start);
    }

    private void ApplyVectors(ushort loadAddress, CodyLoadOptions options)
    {
        if (options.ResetVectorOverride.HasValue)
            SetResetVector(options.ResetVectorOverride.Value);
        else if (options.AutoSetResetVector)
            SetResetVector(loadAddress);

        if (options.IrqVectorOverride.HasValue)
            SetIrqVector(options.IrqVectorOverride.Value);
        if (options.NmiVectorOverride.HasValue)
            SetNmiVector(options.NmiVectorOverride.Value);
    }

    private void SetResetVector(ushort address)
    {
        Memory.ForceWrite(0xFFFC, (byte)(address & 0xFF));
        Memory.ForceWrite(0xFFFD, (byte)(address >> 8));
    }

    private void SetIrqVector(ushort address)
    {
        Memory.ForceWrite(0xFFFE, (byte)(address & 0xFF));
        Memory.ForceWrite(0xFFFF, (byte)(address >> 8));
    }

    private void SetNmiVector(ushort address)
    {
        Memory.ForceWrite(0xFFFA, (byte)(address & 0xFF));
        Memory.ForceWrite(0xFFFB, (byte)(address >> 8));
    }

    public CodyStatusSnapshot GetStatusSnapshot()
    {
        var runStatus = RunStatus.Running;
        if (!Cpu.Run)
            runStatus = RunStatus.Paused;
        return new CodyStatusSnapshot
        {
            RunStatus = runStatus,
            ProfilerSnapshot = Profiler?.LastSnapshot
        };
    }
}
