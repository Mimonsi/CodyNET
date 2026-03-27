namespace CodyNET.Common.Utils;

public static class FileUtils
{
    public static FileInfo GetWithChangedExtension(FileInfo input, string extension)
    {
        return new FileInfo(Path.ChangeExtension(input.FullName, extension));
    }
}