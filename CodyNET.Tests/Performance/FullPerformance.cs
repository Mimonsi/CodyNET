using CodyNET.Common.Utils;
using CodyNET.Core.Cody;
using CodyNET.Host;
using NUnit.Framework;

namespace CodyNET.Tests.Performance;

public class FullPerformance
{
    [Explicit]
    [Test]
    public void TestFullPerformance()
    {
        var setupOptions = new CodySetupOptions
        {
            EnableDebugger = false,
            EnableProfiler = true,
            EnableScreen = false,
            FrequencyHz = -1, // Run as fast as possible for performance testing
        };
        var cody = CodyFactory.CreateCody(setupOptions);
        cody.Boot();
    }
}