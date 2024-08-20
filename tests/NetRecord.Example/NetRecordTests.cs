using Microsoft.Extensions.DependencyInjection;
using NetRecord.Services;
using NetRecord.Services.Extensions;
using NetRecord.Utils.Enums;
using NUnit.Framework;

namespace NetRecord.Example;

public class Tests
{
    private IServiceProvider ServiceProvider { get; set; }

    private IHttpClientFactory _httpFactory;
        
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        var services = new ServiceCollection();

        var config = new NetRecordConfiguration()
        {
            Mode = ServiceMode.Record,
            RecordingsDir = () => "./static"
        };
        services.AddNetRecordHttpClient("client", "https://advocacyday.dev", config);

        ServiceProvider = services.BuildServiceProvider();
    }
    
    [SetUp]
    public void Setup()
    {
        _httpFactory = ServiceProvider.GetRequiredService<IHttpClientFactory>();
    }

    [Test]
    public async Task TestMessageRecordsProperly()
    {
        var client = _httpFactory.CreateClient("client");

        var response = await client.GetAsync("/v5/clients");

        var breakLine = response.Content;
    }
}