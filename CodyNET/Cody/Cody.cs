using CodyNET.Assembler;
using CodyNET.Devices;
using CodyNET.Utils;

namespace CodyNET.Cody;

public class Cody
{
    private Cpu cpu;
    private Memory Memory => cpu.Memory;
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
    public void LoadProgram(byte[] program)
    {
        cpu.LoadRam(program, 0x600);
    }

    public void Start()
    {
        // Open Screen Window and begin rendering
        // Load Cody Basic
        // Start CPU Execution
    }
}