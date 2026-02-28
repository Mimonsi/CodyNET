using CodyNET.Common.Utils;

namespace CodyNET.Core.Roms;

public static class RomLoader
{
    /// <summary>
    /// Loads the embedded CodyBASIC ROM from the CodyNET.Core assembly.
    /// </summary>
    public static byte[] LoadEmbeddedCodyBasic()
    {
        var asm = typeof(RomLoader).Assembly;

        var resources = asm.GetManifestResourceNames();
        Log.Verbose("Available embedded resources: {resources}", string.Join(", ", resources));
        var resourceName = resources.FirstOrDefault(n => n.EndsWith("roms.codybasic.bin", StringComparison.OrdinalIgnoreCase) || n.EndsWith("codybasic.bin", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
            throw new FileNotFoundException("Embedded ROM 'codybasic.bin' not found in assembly resources.");

        using var stream = asm.GetManifestResourceStream(resourceName);
        if (stream is null)
            throw new InvalidOperationException($"Failed to open embedded ROM stream '{resourceName}'.");

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}