using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetRecord.Services;
using NetRecord.Services.Extensions;
using NetRecord.Utils;
using NetRecord.Utils.Enums;
using NUnit.Framework;

namespace NetRecord.Example.Domain;

public class AutoTests : TestSetup
{
    private IHttpClientFactory _httpFactory;
    private NetRecordConfiguration APConfig;
    private NetRecordConfiguration SoapBoxConfig;

    public override IServiceProvider ConfigureServices(IServiceCollection services)
    {
        APConfig = NetRecordConfiguration.Create(ServiceMode.Auto, TestsStaticDir + "/APClient");

        SoapBoxConfig = NetRecordConfiguration.Create(
            ServiceMode.Auto,
            TestsStaticDir + "/SoapBoxClient",
            recordingName: "SoapBoxRecording",
            fileGroupIdentifier: transaction => transaction.Request.Method.Method
        );

        services.AddNetRecordHttpClient("APClient", "https://advocacyday.dev", APConfig);
        services.AddNetRecordHttpClient(
            "soapboxClient",
            "https://soapbox.senate.gov/api/active_offices/?format=json",
            SoapBoxConfig
        );

        return services.BuildServiceProvider();
    }

    [SetUp]
    public void Setup()
    {
        _httpFactory = ServiceProvider.GetRequiredService<IHttpClientFactory>();
    }

    [Test]
    public async Task TestMessageRecordsAndReplaysProperly()
    {
        var apClient = _httpFactory.CreateClient("APClient");
        var soapBoxClient = _httpFactory.CreateClient("soapboxClient");

        var apRecordResponse = await apClient.GetAsync("/v5/clients");
        var soapboxRecordResponse = await soapBoxClient.GetAsync("");

        var testStaticsPath = Path.Join(DirectoryUtils.GetRootPath(), TestsStaticDir);
        Assert.Multiple(() =>
        {
            Assert.That(
                File.Exists(Path.Join(testStaticsPath, "APClient/NetRecordRecording.json")),
                Is.True
            );
            Assert.That(
                File.Exists(
                    Path.Join(testStaticsPath, "SoapBoxClient/SoapBoxRecording_Method_GET.json")
                ),
                Is.True
            );
        });

        var apReplayResponse = await apClient.GetAsync("/v5/clients");
        var soapboxReplayResponse = await soapBoxClient.GetAsync("");

        var apRecordContent = await apRecordResponse.Content.ReadAsStringAsync();
        apRecordContent = JsonSerializer.Serialize(
            JsonSerializer.Deserialize<object>(apRecordContent),
            APConfig.JsonSerializerOptions
        );

        var soapboxRecordContent = await soapboxRecordResponse.Content.ReadAsStringAsync();
        soapboxRecordContent = JsonSerializer.Serialize(
            JsonSerializer.Deserialize<object>(soapboxRecordContent),
            SoapBoxConfig.JsonSerializerOptions
        );

        Assert.Multiple(async () =>
        {
            Assert.That(apReplayResponse.StatusCode, Is.EqualTo(apRecordResponse.StatusCode));
            Assert.That(
                await apReplayResponse.Content.ReadAsStringAsync(),
                Is.EqualTo(apRecordContent)
            );
            Assert.That(
                soapboxReplayResponse.StatusCode,
                Is.EqualTo(soapboxRecordResponse.StatusCode)
            );
            Assert.That(
                await soapboxReplayResponse.Content.ReadAsStringAsync(),
                Is.EqualTo(soapboxRecordContent)
            );
        });

        var file = await File.ReadAllTextAsync(
            Path.Join(testStaticsPath, "APClient/NetRecordRecording.json")
        );
        var soapFile = await File.ReadAllTextAsync(
            Path.Join(testStaticsPath, "SoapBoxClient/SoapBoxRecording_Method_GET.json")
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
