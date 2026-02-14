using CodyNET.Assembler;
using CodyNET.Common.Utils;
using CodyNET.Core.Devices;

namespace CodyNET.Core.Cody;

public sealed class CodyRunOptions
{
    public long FrequencyHz { get; init; } = 1_000_000; // 1 MHz, -1 = as fast as possible
    public bool EnableDebugger { get; init; } = true;
    public bool AsCartridge { get; init; } = false;
    public ushort LoadAddress { get; init; } = 0xE000;
    public ushort? ResetVectorOverride { get; init; } = null;
    public ushort? IrqVectorOverride { get; init; } = null;
    public ushort? NmiVectorOverride { get; init; } = null;
}

public class Cody
{
    private Cpu cpu;
    private Memory Memory => cpu.Memory;
    public long FrequencyHz
    {
        get => cpu.CyclesPerSecond;
        set => cpu.CyclesPerSecond = value;
    }

    //private Screen screen;
    public Cody()
    {
        cpu = new Cpu(); // TODO: Find single way to handle cpu initialization
    }

    /// <summary>
    /// Execute binary file with default options. Sets reset vector to load address
    /// </summary>
    /// <param name="file">binary file to execute (not a cartridge!)</param>
    public void ExecuteBinaryFile(FileInfo file)
    {
        (ushort loadAddress, byte[] bytes) = Binary.LoadBinary(file.FullName, asCartridge: false, overrideLoadAddress: null);
        var options = new CodyRunOptions()
        {
            LoadAddress = loadAddress,
        };
        ExecuteBinary(bytes, options);
    }

    public void ExecuteBinary(byte[] bytes, CodyRunOptions options)
    {
        // 1) Build machine
        cpu = new Cpu
        {
            CyclesPerSecond = options.FrequencyHz
        };

        // 2) Determine image and load address
        var loadAddress = options.LoadAddress;
        byte[] image = bytes;
        if (options.AsCartridge)
        {            
            // Load header
            if (bytes.Length < 4)
                throw new InvalidDataException("Cartridge header must be at least 4 bytes.");

            
            ushort start = (ushort)(bytes[0] | (bytes[1] << 8));
            ushort end   = (ushort)(bytes[2] | (bytes[3] << 8));

            int len = end - start + 1;
            if (len < 0)
                throw new InvalidDataException("Cartridge header invalid (end < start).");

            if (bytes.Length < 4 + len)
                throw new InvalidDataException("Cartridge image truncated (data shorter than header claims).");
            
            // Cartridges have a 4-byte header
            loadAddress = start; // little-endian start address
            image = bytes.Skip(4).Take(len).ToArray();
        }
        
        // 3) Load image
        LoadImage(image, loadAddress);
        
        // 4) Set vectors if overrides provided
        if (options.ResetVectorOverride.HasValue)
            SetResetVector(options.ResetVectorOverride.Value);
        else if (options.AsCartridge)
            SetResetVector(loadAddress); // Rust-like default for cartridge
        if (options.IrqVectorOverride.HasValue)
            SetIrqVector(options.IrqVectorOverride.Value);
        if (options.NmiVectorOverride.HasValue)
            SetNmiVector(options.NmiVectorOverride.Value);
        
        // 5) Reset CPU so it starts executing from reset vector
        cpu.Reset();
        
        // Setup devices
        
        // Run
        cpu.RunUntilFinish();

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

    // TODO: Rework this method
    [Obsolete("Use ExecuteBinary with options instead")]
    public void RunAssemblyFile(string path)
    {
        cpu = new Cpu();
        var program = TassAssembler.AssembleFile(path);
        LoadImage(program, 0xE000);
        // TODO: Check if reset works
        cpu.RunUntilFinish();
    }

    /// <summary>
    /// Loads a binary program into memory at given address
    /// </summary>
    /// <param name="data"></param>
    /// <param name="loadAddress"></param>
    /// 
    public void LoadImage(byte[] data, ushort loadAddress)
    {
        Memory.LoadBytes(data, loadAddress);
    }

    public void Start(bool enableDebugger = true)
    {
        cpu = new Cpu();
        
        if (enableDebugger)
        {
            var debugger = new Debugger(cpu);
            cpu.Memory.RegisterDevice(debugger);
        }
    }

    public long SingleStep() // Performance Testing only
    {
        cpu.Step();
        long cycles = cpu.TotalCyclesExecuted;
        cpu.TotalCyclesExecuted = 0;
        return cycles;
    }

    /// <summary>
    /// Boots the machine by loading the built-in CodyBASIC rom
    /// </summary>
    public void Boot()
    {
        cpu = new Cpu();
        string basicRomPath = "./roms/codybasic.bin"; // TODO: Fix path
        var basicRom = new FileInfo(basicRomPath);
        if (!basicRom.Exists)
            throw new FileNotFoundException($"CodyBASIC ROM not found at path: {basicRomPath}");
        ExecuteBinaryFile(basicRom);
    }
}