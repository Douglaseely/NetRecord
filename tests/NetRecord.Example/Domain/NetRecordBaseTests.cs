using Microsoft.Extensions.DependencyInjection;
using NetRecord.Services;
using NetRecord.Services.Extensions;
using NetRecord.Utils;
using NetRecord.Utils.Enums;
using NetRecord.Utils.Models;
using NUnit.Framework;

namespace NetRecord.Example.Domain;

public class NetRecordBaseTests : TestSetup
{
    private IHttpClientFactory _httpFactory;
    
    public override IServiceProvider ConfigureServices(IServiceCollection services)
    {
        var APConfig = NetRecordConfiguration.Create(ServiceMode.Record, "tests/static/APClient",
            fileGroupIdentifier: transaction => transaction.Request.Method);

        var soapboxConfig = NetRecordConfiguration.Create(ServiceMode.Record, "tests/static/SoapBoxClient",
            fileGroupIdentifier: transaction => transaction.Response.Body!);
        
        services.AddNetRecordHttpClient("APClient", "https://advocacyday.dev", APConfig);
        services.AddNetRecordHttpClient("soapboxClient", "https://soapbox.senate.gov/api/active_offices/?format=json", soapboxConfig);

        return services.BuildServiceProvider();
    }

    [SetUp]
    public void Setup()
    {
        _httpFactory = ServiceProvider.GetRequiredService<IHttpClientFactory>();
    }

    [Test]
    public async Task TestMessageRecordsProperly()
    {
        var apClient = _httpFactory.CreateClient("APClient");
        var soapBoxClient = _httpFactory.CreateClient("soapboxClient");

        var apResponse = await apClient.GetAsync("/v5/clients");
        var soapboxResponse = await soapBoxClient.GetAsync("");

        var testStaticsPath = Path.Join(DirectoryUtils.GetRootPath(), "tests/static");
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Join(testStaticsPath, "/APClient/NetRecordRecording_Method_GET.json")), Is.True);
            Assert.That(Directory.GetFiles(Path.Join(testStaticsPath, "SoapBoxClient")), Is.Not.Empty);
        });
    }
}
