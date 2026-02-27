namespace CodyNET.Common.Video;

public readonly record struct VideoFrame(int Width, int Height, uint[] Pixels);
