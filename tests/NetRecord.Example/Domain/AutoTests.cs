using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetRecord.Services;
using NetRecord.Services.Extensions;
using NetRecord.Utils;
using NetRecord.Utils.Enums;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace NetRecord.Example.Domain;

public class AutoTests : TestSetup
{
    private IHttpClientFactory _httpFactory;

    public override IServiceProvider ConfigureServices(IServiceCollection services)
    {
        var APConfig = NetRecordConfiguration.Create(
            ServiceMode.Auto,
            TestsStaticDir + "/APClient"
        );

        var soapboxConfig = NetRecordConfiguration.Create(
            ServiceMode.Auto,
            TestsStaticDir + "/SoapBoxClient",
            recordingName: "SoapBoxRecording",
            fileGroupIdentifier: transaction => transaction.Request.Method.Method
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

        Assert.Multiple(async () =>
        {
            Assert.That(apReplayResponse.StatusCode, Is.EqualTo(apRecordResponse.StatusCode));
            Assert.That(
                await apReplayResponse.Content.ReadAsStringAsync(),
                Is.EqualTo(await apRecordResponse.Content.ReadAsStringAsync())
            );
            Assert.That(
                soapboxReplayResponse.StatusCode,
                Is.EqualTo(soapboxRecordResponse.StatusCode)
            );
            Assert.That(
                await soapboxReplayResponse.Content.ReadAsStringAsync(),
                Is.EqualTo(await soapboxRecordResponse.Content.ReadAsStringAsync())
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
