namespace CodyPrototype;

class Program
{
    static void Main(string[] args)
    {
        var cpu = new Cpu65C02();

        if (args.Length == 1) // Load file
        {
            byte[] program = File.ReadAllBytes(args[0]);
            cpu.LoadProgram(program, 0x0600);
        }
        else
        {
            cpu.Memory[0x0600] = 0xA9; // LDA #$42
            cpu.Memory[0x0601] = 0x42;
            cpu.Memory[0x0602] = 0x8D; // STA $0200
            cpu.Memory[0x0603] = 0x00;
            cpu.Memory[0x0604] = 0x02;
            cpu.Memory[0x0605] = 0xEA; // NOP
            cpu.Memory[0x0606] = 0x4C; // JMP $0605
            cpu.Memory[0x0607] = 0x05;
            cpu.Memory[0x0608] = 0x06;

            cpu.Reset(0x0600);
        }
        
        Console.WriteLine("Starting CPU...");
        for (int i = 0; i < 20; i++)
        {
            Console.WriteLine($"PC={cpu.PC:X4} A={cpu.A:X2}");
            cpu.Step();
        }
    }
}