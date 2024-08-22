using NetRecord.Interfaces;
using NetRecord.Utils.Exceptions;

namespace NetRecord.Services;

public class NetRecordFactory(
    IEnumerable<INetRecordConfiguration> configurations,
    IEnumerable<INetRecordClientOptions> clientOptions
) : IHttpClientFactory
{
    public HttpClient CreateClient(string name = "")
    {
        var configuration = (NetRecordConfiguration?)
            configurations.FirstOrDefault(config => config.ClientName == name);

        if (configuration is null)
            throw new NetRecordException(
                $"Could not find configuration for HttpClient with name: ({name})"
            );

        var options = clientOptions.FirstOrDefault(options => options.ClientName == name);

        if (options is null)
            throw new NetRecordException(
                $"HttpClient with name: ({name}) not found when attempting request"
            );

        var newClient = NetRecordHttpClient.CreateFromConfiguration(
            options.BaseAddress,
            configuration
        );

        return newClient;
    }
}
