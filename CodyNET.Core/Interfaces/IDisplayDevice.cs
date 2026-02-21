namespace CodyNET.Core.Interfaces;

/// <summary>
/// Screen implementations should implement this interface, RenderFrame will be called whenever the screen needs to update
/// </summary>
public interface IDisplayDevice
{
    public void RenderFrame(VideoFrame frame);
}
