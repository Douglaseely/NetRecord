using Microsoft.Extensions.DependencyInjection;
using NetRecord.Services;
using NetRecord.Services.Extensions;
using NetRecord.Utils.Enums;
using NUnit.Framework;

namespace NetRecord.Example.Domain;

public class ReplayTests : TestSetup
{
    private IHttpClientFactory _httpFactory;

    public override IServiceProvider ConfigureServices(IServiceCollection services)
    {
        var APRecordConfig = NetRecordConfiguration.Create(
            ServiceMode.Record,
            TestsStaticDir + "/APClient"
        );

        var soapboxRecordConfig = NetRecordConfiguration.Create(
            ServiceMode.Record,
            TestsStaticDir + "/SoapBoxClient",
            recordingName: "SoapBoxRecording",
            fileGroupIdentifier: transaction => transaction.Request.Method.Method
        );

        services.AddNetRecordHttpClient(
            "APRecordClient",
            "https://advocacyday.dev",
            APRecordConfig
        );
        services.AddNetRecordHttpClient(
            "soapboxRecordClient",
            "https://soapbox.senate.gov/api/active_offices/?format=json",
            soapboxRecordConfig
        );

        var APReplayConfig = NetRecordConfiguration.Create(
            ServiceMode.Replay,
            TestsStaticDir + "/APClient"
        );

        var soapboxReplayConfig = NetRecordConfiguration.Create(
            ServiceMode.Replay,
            TestsStaticDir + "/SoapBoxClient",
            recordingName: "SoapBoxRecording",
            fileGroupIdentifier: transaction => transaction.Request.Method.Method
        );

        services.AddNetRecordHttpClient(
            "APReplayClient",
            "https://advocacyday.dev",
            APReplayConfig
        );
        services.AddNetRecordHttpClient(
            "soapboxReplayClient",
            "https://soapbox.senate.gov/api/active_offices/?format=json",
            soapboxReplayConfig
        );

        return services.BuildServiceProvider();
    }

    [SetUp]
    public void Setup()
    {
        _httpFactory = ServiceProvider.GetRequiredService<IHttpClientFactory>();
    }

    [Test]
    public async Task TestMessageReplaysProperly()
    {
        var apRecordClient = _httpFactory.CreateClient("APRecordClient");
        var soapBoxRecordClient = _httpFactory.CreateClient("soapboxRecordClient");

        var apRecordResponse = await apRecordClient.GetAsync("/v5/clients");
        var soapboxRecordResponse = await soapBoxRecordClient.GetAsync("");

        var apReplayClient = _httpFactory.CreateClient("APReplayClient");
        var soapBoxReplayClient = _httpFactory.CreateClient("soapboxReplayClient");

        var apReplayResponse = await apReplayClient.GetAsync("/v5/clients");
        var soapboxReplayResponse = await soapBoxReplayClient.GetAsync("");

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
    }
}
