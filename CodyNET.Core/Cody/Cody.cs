using CodyNET.Assembler;
using CodyNET.Common.Video;
using CodyNET.Common.Utils;
using CodyNET.Core.Devices;
using CodyNET.Core.Interfaces;
using Debugger = CodyNET.Core.Devices.Debugger;

namespace CodyNET.Core.Cody;

// Options when creating a new cody, filled with default values for boot command
public sealed record CodySetupOptions
{
    public static CodySetupOptions Default => new();

    public static CodySetupOptions Headless => new()
    {
        FrequencyHz = -1,
        EnableDebugger = false,
        EnableVideo = false,
        EnableKeyboard = false,
    };
    
    public long FrequencyHz { get; init; } = 1_000_000; // 1 MHz, -1 = as fast as possible
    public bool EnableDebugger { get; init; } = true;
    public bool EnableVideo { get; init; } = true;
    public bool EnableKeyboard { get; init; } = true;
}

public sealed record CodyLoadOptions
{
    public static CodyLoadOptions Default => new();
    public bool AsCartridge { get; init; }
    public ushort LoadAddress { get; init; } = 0xE000;
    public bool AutoSetResetVector { get; init; } = true; // If true, sets the reset vector to the load address (unless ResetVectorOverride is set)
    public ushort? ResetVectorOverride { get; init; }
    public ushort? IrqVectorOverride { get; init; }
    public ushort? NmiVectorOverride { get; init; }
}

public sealed record CodyRunOptions
{
    public static CodyRunOptions Default => new();
    public long FrequencyHz { get; init; } = 1_000_000;
    public bool EnableDebugger { get; init; } = false;
    public bool EnableVideo { get; init; } = false;
    public bool AsCartridge { get; init; } = false;
    public ushort LoadAddress { get; init; } = 0xE000;
    public bool AutoSetResetVector { get; init; } = true;
    public ushort? ResetVectorOverride { get; init; } = null;
    public ushort? IrqVectorOverride { get; init; } = null;
    public ushort? NmiVectorOverride { get; init; } = null;
    public string? BasicRomPath { get; init; } = null;
}

public sealed class Cody
{
    private CodySetupOptions setupOptions = new();
    private bool isSetup;

    public Cpu Cpu;
    public Memory Memory => Cpu.Memory;
    public Debugger? Debugger { get; private set; }
    public IVideoDevice? Video { get; private set; }
    public IDisplayDevice? Screen { get; private set; }
    public Keyboard? Keyboard { get; private set; }

    public long FrequencyHz
    {
        get => Cpu.CyclesPerSecond;
        set
        {
            setupOptions = setupOptions with { FrequencyHz = value };
            Cpu.CyclesPerSecond = value;
        }
    }

    public bool FastMode
    {
        get => Cpu.CyclesPerSecond == -1;
        set => FrequencyHz = value ? -1 : 1_000_000;
    }

    public Cody(CodySetupOptions options)
    {
        setupOptions = options;
        
        // 1. Set up CPU (and memory)
        // CPU also initializes memory
        Cpu = new Cpu
        {
            CyclesPerSecond = options.FrequencyHz
        };

        // 2. Set up devices
        if (options.EnableDebugger)
        {
            // watch addresses 0xA000 - 0xA3E7 (text screen)
            var addresses = new List<ushort>();
            for (ushort addr = 0xA000; addr <= 0xA3E7; addr++)
                addresses.Add(addr);
            Debugger = new Debugger(Cpu)
            {
                WatchAddresses = addresses,
                Breakpoints = [0xFD93]
            };
            Memory.RegisterDevice(Debugger);
            Memory.RegisterTap(Debugger);
        }

        if (options.EnableVideo)
        {
            // TODO: Init video
            // Video = new VideoDevice();
            // Cpu.Memory.RegisterDevice(Video);
            Video = new VideoDevice();
            Memory.RegisterDevice(Video);
            Screen = new PpmVideoOutput();
        }

        if (options.EnableKeyboard)
        {
            // TODO: Init keyboard
            // Keyboard = new Keyboard();
            // Cpu.Memory.RegisterDevice(Keyboard);
        }
    }

    public Cody() : this(CodySetupOptions.Default)
    {
        Log.Info("Cody initialized with default setup options.");
    }

    public void Reset()
    {
        Cpu.Reset();
    }

    public StepResult Step()
    {
        return Cpu.Step();
    }

    public void RunUntilFinish()
    {
        //Cpu.RunUntilFinish();
        VideoDevice vid = (VideoDevice) Video!;
        
        Log.Level = LogLevel.Debug;
        while (true)
        {
            Cpu.Step();
            // TEMP
            if (Debugger is not null && Debugger.IsAtBreakpoint())
            {
                Log.Info("Execution paused by debugger on PC={Cpu.PC:X4}");
                //break;
            }
            if (Video != null && vid.Dirty) // TODO: Only write when video is dirty
            {
                var frame = Video.RenderTextFrame(Cpu.Memory);
                Screen.RenderFrame(frame);
            }
        }
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
    
    private void CheckFilePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("File path must not be empty.", nameof(path));
        if (!File.Exists(path))
            throw new FileNotFoundException("File not found.", path);
    }

    public void LoadBinaryFile(string filePath, CodyLoadOptions options)
    {
        CheckFilePath(filePath);

        var bytes = File.ReadAllBytes(filePath);
        LoadBinary(bytes, options);
    }
    
    public void LoadBinary(byte[] bytes, CodyLoadOptions? options = null)
    {
        if (bytes is null) throw new ArgumentNullException(nameof(bytes));
        
        options ??= new CodyLoadOptions();

        var (image, loadAddress) = ParseImage(bytes, options);
        LoadImage(image, loadAddress);
        ApplyVectors(loadAddress, options);
    }

    public void LoadAssemblyFile(string filePath, CodyLoadOptions? options = null)
    {
        CheckFilePath(filePath);

        var program = TassAssembler.AssembleFile(filePath);
        LoadBinary(program, options);
    }
    
    public void RunBinaryFile(string filePath, CodyLoadOptions loadOptions)
    {
        Log.Debug("Running binary file: '{filePath}'", filePath);
        CheckFilePath(filePath);

        var bytes = File.ReadAllBytes(filePath);
        RunBinary(bytes, loadOptions);
    }
    
    public void RunBinary(byte[] bytes, CodyLoadOptions loadOptions)
    {
        LoadBinary(bytes, loadOptions);
        Reset();
        RunUntilFinish();
    }

    public void RunAssemblyFile(string filePath, CodyLoadOptions? loadOptions=null)
    {
        CheckFilePath(filePath);
        
        LoadAssemblyFile(filePath, loadOptions ?? CodyLoadOptions.Default);
        Reset();
        RunUntilFinish();
    }
    
    /// <summary>
    /// Boots the machine by loading the built-in CodyBASIC ROM.
    /// </summary>
    public void Boot(string? basicRomPath = "codybasic.bin")
    {
        Log.Debug("Booting Cody with CodyBASIC ROM.");
        var resolvedPath = ResolveBasicRomPath(
            basicRomPath);
        Log.Trace("Resolved CodyBASIC ROM path: {path}", resolvedPath);

        var loadOptions = CodyLoadOptions.Default with
        {
            AutoSetResetVector = false
        };

        RunBinaryFile(resolvedPath, loadOptions);
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

    private static string ResolveBasicRomPath(string? configuredPath)
    {
        _ = configuredPath; // ROM path is fixed by convention.
        var path = Path.Combine(AppContext.BaseDirectory, "roms", "codybasic.bin");
        if (File.Exists(path))
            return path;

        throw new FileNotFoundException(
            $"CodyBASIC ROM not found at fixed path: {path}");
    }
}
