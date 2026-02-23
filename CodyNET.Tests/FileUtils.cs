namespace CodyNET.Tests;

public static class FileUtils
{
    public static string GetTestDataPath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "testdata", fileName);
    }
    
    public static FileInfo GetTestDataFile(string fileName)
    {
        return new FileInfo(GetTestDataPath(fileName));
    }
}