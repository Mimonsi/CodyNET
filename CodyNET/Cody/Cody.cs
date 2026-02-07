using CodyNET.Assembler;
using CodyNET.Devices;
using CodyNET.Utils;

namespace CodyNET.Cody;

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
        cpu = new Cpu();
        Debugger debugger = new Debugger(cpu);
        Memory.RegisterDevice(debugger);
        //screen = new Screen();
    }

    public void RunAssemblyFile(string path)
    {
        var program = TassAssembler.AssembleFile(path);
        LoadProgram(program);
        cpu.RunUntilFinish();
    }
    
    /// <summary>
    /// Loads a binary program into memory at address 0x0600
    /// </summary>
    /// <param name="program"></param>
    public void LoadProgram(byte[] program, ushort loadAddress = 0x0600)
    {
        cpu.LoadRam(program, loadAddress);
    }

    public void Start()
    {
        // Open Screen Window and begin rendering
        // Load Cody Basic
        // Start CPU Execution
    }

    public long SingleStep() // Performance Testing only
    {
        cpu.Step();
        long cycles = cpu.TotalCyclesExecuted;
        cpu.TotalCyclesExecuted = 0;
        return cycles;
    }
}