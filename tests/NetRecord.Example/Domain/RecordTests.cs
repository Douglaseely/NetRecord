using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetRecord.Services;
using NetRecord.Services.Extensions;
using NetRecord.Utils;
using NetRecord.Utils.Enums;
using NUnit.Framework;

namespace NetRecord.Example.Domain;

public class RecordTests : TestSetup
{
    private IHttpClientFactory _httpFactory;

    public override IServiceProvider ConfigureServices(IServiceCollection services)
    {
        var APConfig = NetRecordConfiguration.Create(
            ServiceMode.Record,
            TestsStaticDir + "/APClient"
        );

        var soapboxConfig = NetRecordConfiguration.Create(
            ServiceMode.Record,
            TestsStaticDir + "/SoapBoxClient",
            recordingName: "SoapBoxRecording",
            fileGroupIdentifier: transaction => transaction.Request.RequestHeaders["Random"]
        );

        services.AddNetRecordHttpClient("APClient", "https://advocacyday.dev", APConfig);
        services.AddNetRecordHttpClient(
            "soapboxClient",
            "https://soapbox.senate.gov/api/active_offices/?format=json",
            soapboxConfig
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
        var apClient = _httpFactory.CreateClient("APClient");
        var soapBoxClient = _httpFactory.CreateClient("soapboxClient");

        var soapBoxRequest = new HttpRequestMessage(HttpMethod.Get, "");
        soapBoxRequest.Headers.Add("Random", "Hello");
        
        var apResponse = await apClient.GetAsync("/v5/clients");
        var soapboxResponse = await soapBoxClient.SendAsync(soapBoxRequest);

        var testStaticsPath = Path.Join(DirectoryUtils.GetRootPath(), TestsStaticDir);
        Assert.Multiple(() =>
        {
            Assert.That(
                File.Exists(Path.Join(testStaticsPath, "APClient/NetRecordRecording.json")),
                Is.True
            );
            Assert.That(
                File.Exists(
                    Path.Join(testStaticsPath, "SoapBoxClient/SoapBoxRecording_RequestHeaders_Random_Hello.json")
                ),
                Is.True
            );
        });
    }

    [Test]
    public async Task TestMessageRecordOverwritesProperly()
    {
        var apClient = _httpFactory.CreateClient("APClient");
        var soapBoxClient = _httpFactory.CreateClient("soapboxClient");
        
        var soapBoxRequest = new HttpRequestMessage(HttpMethod.Get, "");
        soapBoxRequest.Headers.Add("Random", "Hello");
        
        var soapBoxRequest2 = new HttpRequestMessage(HttpMethod.Get, "");
        soapBoxRequest2.Headers.Add("Random", "Hello");
        
        var soapBoxRequest3 = new HttpRequestMessage(HttpMethod.Get, "");
        soapBoxRequest3.Headers.Add("Random", "Hello");

        var apResponse = await apClient.GetAsync("/v5/clients");
        var soapboxResponse = await soapBoxClient.SendAsync(soapBoxRequest);

        var apResponse2 = await apClient.GetAsync("/v5/clients");
        var soapboxResponse2 = await soapBoxClient.SendAsync(soapBoxRequest2);

        var apResponse3 = await apClient.GetAsync("/v5/clients");
        var soapboxResponse3 = await soapBoxClient.SendAsync(soapBoxRequest3);

        var testStaticsPath = Path.Join(DirectoryUtils.GetRootPath(), TestsStaticDir);
        Assert.Multiple(() =>
        {
            Assert.That(
                File.Exists(Path.Join(testStaticsPath, "APClient/NetRecordRecording.json")),
                Is.True
            );
            Assert.That(
                File.Exists(
                    Path.Join(testStaticsPath, "SoapBoxClient/SoapBoxRecording_RequestHeaders_Random_Hello.json")
                ),
                Is.True
            );
        });

        var file = await File.ReadAllTextAsync(
            Path.Join(testStaticsPath, "APClient/NetRecordRecording.json")
        );
        var soapFile = await File.ReadAllTextAsync(
            Path.Join(testStaticsPath, "SoapBoxClient/SoapBoxRecording_RequestHeaders_Random_Hello.json")
        );
        var serializedList = JsonSerializer.Deserialize<List<object>>(file);
        var soapSerializedList = JsonSerializer.Deserialize<List<object>>(soapFile);
        Assert.Multiple(() =>
        {
            Assert.That(serializedList?.Count, Is.EqualTo(1));
            Assert.That(soapSerializedList?.Count, Is.EqualTo(1));
        });
    }
}
