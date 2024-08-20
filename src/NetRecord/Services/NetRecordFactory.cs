using NetRecord.Interfaces;
using NetRecord.Utils.Exceptions;

namespace NetRecord.Services;

public class NetRecordFactory(INetRecordConfiguration configuration, IEnumerable<INetRecordClientOptions> clientOptions) : IHttpClientFactory
{
    private readonly NetRecordConfiguration _netRecordConfiguration = (NetRecordConfiguration)configuration;
    private static NetRecordHttpClient? _httpClient = null;
    
    // This dictionary will hold the names of the clients and their associated base addresses
    public HttpClient CreateClient(string name = "")
    {
        if (configuration.FactoryReturnsSingleClient && _httpClient is not null)
            return _httpClient;

        var options = clientOptions.FirstOrDefault(options => options.ClientName == name);

        if (options is null)
            throw new NetRecordException($"HttpClient with name: ({name}) not found when attempting request");

        var newClient = NetRecordHttpClient.CreateFromConfiguration(options.BaseAddress, _netRecordConfiguration);

        _httpClient = newClient;
        return newClient;
    }
}