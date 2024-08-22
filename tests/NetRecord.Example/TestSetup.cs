using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace NetRecord.Example;

public abstract class TestSetup
{
    private static IServiceCollection ServiceCollection { get; } = new ServiceCollection();

    private IServiceProvider ServiceProvider { get; }

    public abstract IServiceProvider ConfigureServices(IServiceCollection services);

    public IServiceScope Scope { get; private set; }
    public TestFixture TestFixture { get; } = TestFixture.Instance;

    public TestSetup()
    {
        RemoveServices(ServiceCollection);

        // ReSharper disable once VirtualMemberCallInConstructor
        ServiceProvider = ConfigureServices(ServiceCollection);
    }

    [SetUp]
    public virtual async Task RunBeforeEveryTest()
    {
        Scope = ServiceProvider.CreateScope();
    }

    [TearDown]
    public virtual async Task RunAfterEveryTest()
    {
        Scope.Dispose();

        if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            TestFixture.FailedTests.Add(TestContext.CurrentContext.Result);
    }

    public async Task SaveAndReset()
    {
        throw new NotImplementedException(
            "This function should be implemented in a derived test class."
        );
    }
    
    private static IServiceCollection RemoveServices(IServiceCollection services, params Type[] except)
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