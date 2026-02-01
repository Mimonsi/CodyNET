using CodyNET.Assembler;
using CodyNET.Utils;

namespace CodyNET.Cody;

public class Cody
{
    private Cpu cpu;

    private ICodyAssembler assembler;
    //private Screen screen;
    public Cody()
    {
        cpu = new Cpu();
        assembler = new TassAssembler();
        //screen = new Screen();
    }

    public void RunAssemblyFile(string path)
    {
        var program = assembler.AssembleFile(path);
        LoadProgram(program);
        RunUntilFinish();
    }
    
    /// <summary>
    /// Loads a binary program into memory at address 0x0600
    /// </summary>
    /// <param name="program"></param>
    public void LoadProgram(byte[] program)
    {
        cpu.LoadRam(program, 0x600);
    }

    public void RunUntilFinish()
    {
        StepResult lastResult = StepResult.Success;
        while (lastResult != StepResult.Finished)
        {
            lastResult = cpu.Step();
        }
        Log.Info("Program finished execution.");
    }

    public void Start()
    {
        // Load start program into memory (cody basic)
        // Open screen window and begin rendering
        // Start cpu execution
    }
}