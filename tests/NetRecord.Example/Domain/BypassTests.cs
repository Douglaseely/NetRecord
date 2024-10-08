using Microsoft.Extensions.DependencyInjection;
using NetRecord.Services;
using NetRecord.Services.Extensions;
using NetRecord.Utils;
using NetRecord.Utils.Enums;
using NUnit.Framework;

namespace NetRecord.Example.Domain;

public class BypassTests : TestSetup
{
    private IHttpClientFactory _httpFactory;

    public override IServiceProvider ConfigureServices(IServiceCollection services)
    {
        var APConfig = NetRecordConfiguration.Create(
            ServiceMode.Bypass,
            TestsStaticDir + "/APClient"
        );

        services.AddNetRecordHttpClient("APClient", "https://advocacyday.dev", APConfig);

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
        var testStaticsPath = Path.Join(DirectoryUtils.GetRootPath(), TestsStaticDir);
        Assert.That(
            File.Exists(Path.Join(testStaticsPath, "APClient/NetRecordRecording.json")),
            Is.False
        );

        var apClient = _httpFactory.CreateClient("APClient");

        var apResponse = await apClient.GetAsync("/v5/clients");

        Assert.That(
            File.Exists(Path.Join(testStaticsPath, "APClient/NetRecordRecording.json")),
            Is.False
        );
    }
}
