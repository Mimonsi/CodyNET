namespace CodyNET.Tests;

public static class FileUtils
{
    public static string GetTestDataPath(string filename)
    {
        return Path.Combine(AppContext.BaseDirectory, "testdata", filename);
    }
}