using CodyNET.Core.Cody;

namespace CodyNET.Core.Interfaces;

public readonly record struct VideoFrame(int Width, int Height, uint[] Pixels);

public interface IVideoDevice : IMemoryMappedDevice
{
    public VideoFrame RenderTextFrame(Memory memory);
}