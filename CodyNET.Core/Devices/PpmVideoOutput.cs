using CodyNET.Common.Video;

namespace CodyNET.Core.Devices;

/// <summary>
/// A simple implementation of a device that "renders" a VideoFrame into a ppm file for external display.
/// </summary>
///  TODO: This should not be in core but in frontend
public class PpmVideoOutput(string filePath = "C:/Users/Konsi/Desktop/screen.ppm") : IScreenDevice
{
    public void RenderFrame(VideoFrame frame)
    {
        PpmCodec.Save(frame, filePath);
    }
}
