using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NetRecord.Utils;
using NUnit.Framework;

namespace NetRecord.Example;

[SetUpFixture]
public class TestFixture
{
    public static TestFixture Instance { get; private set; }

    public List<TestContext.ResultAdapter> FailedTests { get; }

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        // Empty out the test recordings before restoring for a constant state
        var staticDirectory = Path.Join(DirectoryUtils.GetRootPath(), "test/static");
        if (Directory.Exists(staticDirectory))
            Directory.Delete(staticDirectory, true);
        
        Instance = this;
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        Trace.WriteLine(null);
    }

    public async Task<IServiceProvider> ConfigureServices(params Type[] assemblyMarkers)
    {
        var services = new ServiceCollection();

        return services.BuildServiceProvider();
    }
}
