using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Threading;
using CodyNET.Core.Cody;
using CodyNET.Frontend.Controls;

namespace CodyNET.Frontend;

public partial class MainWindow : Window
{
    private Memory? memory;
    private Cpu? cpu;
    private ScreenControl? screen;
    private DispatcherTimer? emulationTimer;
    
    public MainWindow()
    {
        InitializeComponent();
        InitializeEmulator();
    }
    
    private void InitializeEmulator()
    {
        // Create memory
        memory = new Memory();
        
        // Create and register screen
        screen = this.FindControl<ScreenControl>("Screen");
        if (screen != null)
        {
            screen.ScaleFactor = 6.0; // Set desired scale factor
            memory.RegisterDevice(screen);
        }
            
        
        // Create CPU (assuming you have this)
        // cpu = new CPU(memory);
        
        // Test: Write some text to screen
        //TestWriteToScreen();
        TestHeight();
        
        // Start emulation loop
        // StartEmulation();
    }

    private void WriteText(string text, int x, int y)
    {
        ushort baseAddr = (ushort)(0x0200 + (y * 40) + x);
        
        for (int i = 0; i < text.Length; i++)
        {
            memory.Write((ushort)(baseAddr + i), (byte)text[i]);
        }
    }

    private void TestHeight()
    {
        if (memory == null) return;
        for (int i = 0; i < 25; i++)
        {
            if (i == 0)
            {
                for(int j = 0; j < 40; j++)
                {
                    WriteText("*", j, 0);
                }
            }
            WriteText(i.ToString(), i, i);
        }
    }
    
    private void TestWriteToScreen()
    {
        if (memory == null) return;
        
        // Write "CODY COMPUTER" at position (x, y)
        WriteText("CODY COMPUTER", 14, 1);
        
        // Write "Ready." at position (x, y)
        WriteText("Ready.", 2, 2);
        
        WriteText("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNO", 0, 4);
        
        
        // Animated test
        int counter = 0;
        var animTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        animTimer.Tick += (s, e) =>
        {
            string msg = $"Counter: {counter++}";
            int x = 2;
            int y = 6;
            ushort addr = (ushort)(0x0200 + (y * 40) + x);
            for (int i = 0; i < msg.Length; i++)
                memory?.Write((ushort)(addr + i), (byte)msg[i]);
            
            
        };
        animTimer.Start();
    }
    
    private void StartEmulation()
    {
        emulationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        
        emulationTimer.Tick += (s, e) =>
        {
            // Execute CPU cycles
            // cpu?.ExecuteCycles(10000);
        };
        
        emulationTimer.Start();
    }

    private void OnScaleMenuClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (screen == null || sender is not MenuItem menuItem)
        {
            return;
        }

        if (menuItem.Tag is string tagValue
            && double.TryParse(tagValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double scale))
        {
            screen.ScaleFactor = scale;
        }
    }

}