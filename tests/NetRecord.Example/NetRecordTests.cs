using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace NetRecord.Example;

public class Tests
{
    public IServiceProvider ServiceProvider { get; private set; }
    private IServiceCollection ServiceCollection { get; } = new ServiceCollection();
    
    public IServiceScope Scope { get; private set; }
    
    public IRestClient 
        
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        
    }
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}