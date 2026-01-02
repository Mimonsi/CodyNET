using System;
using System.IO;
using CodyPrototype.Assembler;
using CodyPrototype.Utils;

namespace CodyPrototype;

class Program
{
    static void Main(string[] args)
    {
        Test1();
    }

    private static void Test1()
    {
        // Tests breakpoint with other debug DRG behind it
        Log.Info("Starting Test1");
        Cpu.Cpu cpu = new();
        var bytes = GetBytesFromFile("testdata/drs_after_dbp.bin");
        cpu.LoadProgram(bytes, 0x0600);
        cpu.RunUntilFinish();
        Log.Info("Done");
    }
    
    public static byte[] GetBytesFromFile(string filePath)
    {
        var file = Path.Combine(@"C:\Users\Konsi\GoogleDrive\Uni\Bachelorarbeit\CodyNET\CodyPrototype.Tests", filePath);
        // Remove all strings and new lines
        var content = File.ReadAllText(file);
        var byteStrings = content.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries);
        var bytes = new byte[byteStrings.Length];
        for (int i = 0; i < byteStrings.Length; i++)
        {            
            bytes[i] = Convert.ToByte(byteStrings[i], 16);
        }
        return bytes;
    }
}