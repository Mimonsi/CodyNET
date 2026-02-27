using CodyNET.Common.Video;
using CodyNET.Core.Cody;

namespace CodyNET.Core.Interfaces;

public interface IVideoDevice : IMemoryMappedDevice
{
    public VideoFrame RenderTextFrame(Memory memory);
    
    public static VideoFrame TestFrame => new(320, 200, Enumerable.Range(0, 40 * 25).Select(i => (uint)(i * 0x99999999)).ToArray());
}
