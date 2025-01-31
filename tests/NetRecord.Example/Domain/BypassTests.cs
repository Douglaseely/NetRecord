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
        var soapBoxConfig = NetRecordConfiguration.Create(
            ServiceMode.Bypass,
            TestsStaticDir + "/SoapBoxClient",
            recordingName: "SoapBoxRecording",
            fileGroupIdentifier: transaction => transaction.Request.Method.Method
        );

        services.AddNetRecordHttpClient(
            "soapBoxClient",
            "https://soapbox.senate.gov/api/active_offices/?format=json",
            soapBoxConfig
        );

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
            File.Exists(Path.Join(testStaticsPath, "SoapBoxClient/SoapBoxRecording_Method_GET.json")),
            Is.False
        );

        var soapBoxClient = _httpFactory.CreateClient("soapBoxClient");

        var soapBoxResponse = await soapBoxClient.GetAsync("/v5/clients");

        Assert.That(
            File.Exists(Path.Join(testStaticsPath, "SoapBoxClient/SoapBoxRecording_Method_GET.json")),
            Is.False
        );
    }
}
