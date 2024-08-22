using NetRecord.Interfaces;
using NetRecord.Utils.Exceptions;

namespace NetRecord.Services;

public class NetRecordFactory(
    IEnumerable<INetRecordConfiguration> configurations,
    IEnumerable<INetRecordClientOptions> clientOptions
) : IHttpClientFactory
{
    private static NetRecordHttpClient? _httpClient = null;

    // This dictionary will hold the names of the clients and their associated base addresses
    public HttpClient CreateClient(string name = "")
    {
        var configuration = (NetRecordConfiguration?)
            configurations.FirstOrDefault(config => config.ClientName == name);

        if (configuration is null)
            throw new NetRecordException(
                $"Could not find configuration for HttpClient with name: ({name})"
            );

        if (configuration.FactoryReturnsSingleClient && _httpClient is not null)
            return _httpClient;

        var options = clientOptions.FirstOrDefault(options => options.ClientName == name);

        if (options is null)
            throw new NetRecordException(
                $"HttpClient with name: ({name}) not found when attempting request"
            );

        var newClient = NetRecordHttpClient.CreateFromConfiguration(
            options.BaseAddress,
            configuration
        );

        _httpClient = newClient;
        return newClient;
    }
}
