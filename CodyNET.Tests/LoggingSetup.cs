using CodyNET.Common.Utils;
using NUnit.Framework;

namespace CodyNET.Tests;

[SetUpFixture]
public sealed class LoggingSetup
{
    [OneTimeSetUp]
    public void Configure()
    {
        Environment.SetEnvironmentVariable("CODYNET_LOG_DISABLE_FILE", "1");
        Log.Level = LogLevel.Error;
    }
}
