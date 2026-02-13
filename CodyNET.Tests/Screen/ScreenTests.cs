using CodyNET.Core.Cody;
using CodyNET.Core.Devices;
using NUnit.Framework;

namespace CodyNET.Tests.Screen;

public class ScreenTests
{
    public void TestBorderColor()
    {
        //Memory mem = new Memory<>()
        //vid.Write(0xD002, 0x05); // Set border color to green
    }
    
    // HUGE TEST IDEA: Use PNG files to test the VID output. Needed: Frontend that converts bitmap to PNG and vice versa, and a test that compares the output of the VID to a reference PNG file.
    
    [Test]
    public void Vid_RegisterWriteRead_SetsDirty()
    {
        var mem = new Memory();              // wie auch immer du Memory konstruierst
        var vid = new VID();

        Assert.True(vid.Dirty);

        vid.Write(0xD002, 0x05);             // border color
        Assert.True(vid.Dirty);

        var v = vid.Read(0xD002);
        Assert.That(v, Is.EqualTo(0x05));
    }

    [Test]
    public void TestPresenter()
    {
        int width = 10;
        int height = 20;

        var pixels = new uint[width * height];

        // Wir erzeugen 4 horizontale Farbblöcke
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                uint color;

                if (y < 5)
                    color = Rgba(255, 0, 0);       // Rot
                else if (y < 10)
                     color = Rgba(0, 255, 0);       // Grün
                // else if (y < 15)
                //     color = Rgba(0, 0, 255);       // Blau
                else
                    color = Rgba(255, 255, 0);     // Gelb

                pixels[y * width + x] = color;
            }
        }

        ConsoleFramePresenter.PrintFrame(width, height, pixels);
    }
    
    private static uint Rgba(byte r, byte g, byte b, byte a = 255)
    {
        return ((uint)r << 24)
               | ((uint)g << 16)
               | ((uint)b << 8)
               | a;
    }


    
    [Test]
    public void RenderTextFrame_FillsBorderColor()
    {
        var mem = new Memory();
        var vid = new VID();

        vid.Write(0xD002, 0x06); // palette index 6 = BLUE bei dir

        var frame = vid.RenderTextFrame(mem);

        uint expected = ((uint)VID.Color.BLUE.R << 24)
                        | ((uint)VID.Color.BLUE.G << 16)
                        | ((uint)VID.Color.BLUE.B << 8)
                        | 255;
        
        ConsoleFramePresenter.PrintFrame(frame.Width, frame.Height, frame.Pixels);
        
        Assert.That(frame.Pixels[0], Is.EqualTo(expected));
    }


}