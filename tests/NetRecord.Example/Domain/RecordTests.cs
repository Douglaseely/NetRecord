using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetRecord.Services;
using NetRecord.Services.Extensions;
using NetRecord.Utils;
using NetRecord.Utils.Enums;
using NetRecord.Utils.Models;
using NUnit.Framework;

namespace NetRecord.Example.Domain;

public class RecordTests : TestSetup
{
    private IHttpClientFactory _httpFactory;
    
    public override IServiceProvider ConfigureServices(IServiceCollection services)
    {
        var APConfig = NetRecordConfiguration.Create(ServiceMode.Record, "tests/static/APClient");

        var soapboxConfig = NetRecordConfiguration.Create(ServiceMode.Record, "tests/static/SoapBoxClient",
            recordingName: "SoapBoxRecording", fileGroupIdentifier: transaction => transaction.Request.Method.Method);
        
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
            Assert.That(File.Exists(Path.Join(testStaticsPath, "APClient/NetRecordRecording.json")), Is.True);
            Assert.That(File.Exists(Path.Join(testStaticsPath, "SoapBoxClient/SoapBoxRecording_Method_GET.json")), Is.True);
        });
    }
    
    [Test]
    public async Task TestMessageRecordOverwritesProperly()
    {
        var apClient = _httpFactory.CreateClient("APClient");
        var soapBoxClient = _httpFactory.CreateClient("soapboxClient");

        var apResponse = await apClient.GetAsync("/v5/clients");
        var soapboxResponse = await soapBoxClient.GetAsync("");
        
        var apResponse2 = await apClient.GetAsync("/v5/clients");
        var soapboxResponse2 = await soapBoxClient.GetAsync("");
        
        var apResponse3 = await apClient.GetAsync("/v5/clients");
        var soapboxResponse3 = await soapBoxClient.GetAsync("");

        var testStaticsPath = Path.Join(DirectoryUtils.GetRootPath(), "tests/static");
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Join(testStaticsPath, "APClient/NetRecordRecording.json")), Is.True);
            Assert.That(File.Exists(Path.Join(testStaticsPath, "SoapBoxClient/SoapBoxRecording_Method_GET.json")), Is.True);
        });

        var file = await File.ReadAllTextAsync(Path.Join(testStaticsPath, "APClient/NetRecordRecording.json"));
        var soapFile =
            await File.ReadAllTextAsync(Path.Join(testStaticsPath, "SoapBoxClient/SoapBoxRecording_Method_GET.json"));
        var serializedList = JsonSerializer.Deserialize<List<object>>(file);
        var soapSerializedList = JsonSerializer.Deserialize<List<object>>(soapFile);
        Assert.Multiple(() =>
        {
            Assert.That(serializedList?.Count, Is.EqualTo(1));
            Assert.That(soapSerializedList?.Count, Is.EqualTo(1));
        });
    }
}
