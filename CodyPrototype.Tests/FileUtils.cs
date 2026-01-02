namespace CodyPrototype.Utils;

public class FileUtils
{
    /// <summary>
    /// Get bytes from a file containing hex strings separated by spaces/new lines
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static byte[] GetBytesFromFile(string filePath)
    {
        // Remove all strings and new lines
        var content = File.ReadAllText(filePath);
        var byteStrings = content.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries);
        var bytes = new byte[byteStrings.Length];
        for (int i = 0; i < byteStrings.Length; i++)
        {            
            bytes[i] = Convert.ToByte(byteStrings[i], 16);
        }
        return bytes;
    }
}