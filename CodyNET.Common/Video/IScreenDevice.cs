namespace CodyNET.Common.Video;

/// <summary>
/// Screen implementations should implement this interface, RenderFrame will be called whenever the screen needs to update.
/// </summary>
public interface IScreenDevice
{
    public void RenderFrame(VideoFrame frame);
}
