using NetRecord.Interfaces;

namespace NetRecord.Services;

/// <summary>
///
/// </summary>
public class NetRecordHttpClient : HttpClient
{
    internal NetRecordHandler handler { get; }

    internal NetRecordHttpClient(NetRecordHandler handler)
        : base(handler) { }

    /// <summary>
    /// Creates a NetRecordHttpClient using the passed configurations settings
    /// </summary>
    /// <param name="baseAddress">The base address for the httpClient created</param>
    /// <param name="configuration">The passed configuration settings for the HttpClient to use</param>
    /// <returns>A new NetRecordHttpClient</returns>
    public static NetRecordHttpClient CreateFromConfiguration(
        Uri baseAddress,
        NetRecordConfiguration configuration
    )
    {
        var handler = new NetRecordHandler(configuration);
        var client = new NetRecordHttpClient(handler);
        client.BaseAddress = baseAddress;
        return client;
    }

    /// <summary>
    /// Creates a NetRecordHttpClient using the passed configurations settings
    /// </summary>
    /// <param name="configuration">The passed configuration settings for the HttpClient to use</param>
    /// <returns>A new NetRecordHttpClient</returns>
    public static NetRecordHttpClient CreateFromConfiguration(NetRecordConfiguration configuration)
    {
        var handler = new NetRecordHandler(configuration);
        var client = new NetRecordHttpClient(handler);
        return client;
    }

    /// <summary>
    /// Creates a NetRecordHttpClient using the passed configurations settings
    /// </summary>
    /// <param name="baseAddress">The base address for the httpClient created</param>
    /// <param name="configuration">The passed configuration settings for the HttpClient to use</param>
    /// <returns>A new NetRecordHttpClient</returns>
    public static NetRecordHttpClient CreateFromConfiguration(
        Uri baseAddress,
        INetRecordConfiguration configuration
    )
    {
        return CreateFromConfiguration(baseAddress, (NetRecordConfiguration)configuration);
    }
}
