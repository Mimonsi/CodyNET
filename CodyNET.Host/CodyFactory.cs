using CodyNET.Common.Video;
using CodyNET.Core.Cody;
using CodyNET.Core.Devices;
using CodyNET.Core.Interfaces;

namespace CodyNET.Host;

public static class CodyFactory
{
    public static Cody CreateCody(CodySetupOptions options)
    {
        IScreenDevice? screen=null;
        IVideoDevice? video=null;
        if (options.EnableScreen)
        {
            screen = CreateScreen();
            video = new VideoDevice();
        }
        var keyboard = CreateKeyboard();

        Cody cody = new(options, video, screen, keyboard);
        return cody;
    }

    private static IScreenDevice? CreateScreen()
    {
        var screen = new PpmVideoOutput();
        return screen;
    }
    
    private static IInputDevice? CreateKeyboard()
    {
        // TODO: Keyboard initialization
        return null;
    }
}