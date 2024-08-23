using Microsoft.Extensions.DependencyInjection;
using NetRecord.Utils;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace NetRecord.Example;

public abstract class TestSetup
{
    public const string TestsStaticDir = "tests/static";

    private static IServiceCollection ServiceCollection { get; } = new ServiceCollection();

    protected IServiceProvider ServiceProvider { get; }

    public abstract IServiceProvider ConfigureServices(IServiceCollection services);

    public TestFixture TestFixture { get; } = TestFixture.Instance;

    public TestSetup()
    {
        RemoveServices(ServiceCollection);

        // ReSharper disable once VirtualMemberCallInConstructor
        ServiceProvider = ConfigureServices(ServiceCollection);
    }

    [SetUp]
    public virtual async Task RunBeforeEveryTest() { }

    [TearDown]
    public virtual async Task RunAfterEveryTest()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            TestFixture.FailedTests.Add(TestContext.CurrentContext.Result);

        // Empty out the test recordings before restoring for a constant state
        var staticDirectory = Path.Join(DirectoryUtils.GetRootPath(), TestsStaticDir);
        if (Directory.Exists(staticDirectory))
            Directory.Delete(staticDirectory, true);
    }

    private static IServiceCollection RemoveServices(
        IServiceCollection services,
        params Type[] except
    )
    {
        var serviceList = services.ToList().Where(service => !except.Contains(service.ServiceType));

        foreach (var service in serviceList)
        {
            RemoveService(services, service.ServiceType);
        }

        return services;
    }

    private static IServiceCollection RemoveService(IServiceCollection services, Type type)
    {
        var existingService = services.FirstOrDefault(d => d.ServiceType == type);
        if (existingService != null)
            services.Remove(existingService);
        return services;
    }
}
