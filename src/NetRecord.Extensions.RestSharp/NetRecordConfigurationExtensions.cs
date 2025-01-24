using NetRecord.Interfaces;
using NetRecord.Services;
using RestSharp;

namespace NetRecord.Extensions;

public static class NetRecordConfigurationExtensions
{
    /// <summary>
    /// Creates a RestClient that uses a NetRecordHttpClient from the current configuration
    /// </summary>
    /// <param name="baseAddress">The base address to set within the httpClient</param>
    /// <returns>A newly created NetRecordHttpClient</returns>
    public static RestClient CreateRestClient(
        this NetRecordConfiguration configuration,
        Uri baseAddress
    )
    {
        var httpClient = NetRecordHttpClient.CreateFromConfiguration(baseAddress, configuration);
        return new RestClient(httpClient);
    }

    /// <summary>
    /// Creates a RestClient that uses a NetRecordHttpClient from the current configuration
    /// </summary>
    /// <returns>A newly created NetRecordHttpClient</returns>
    public static RestClient CreateRestClient(this NetRecordConfiguration configuration)
    {
        var httpClient = NetRecordHttpClient.CreateFromConfiguration(configuration);
        return new RestClient(httpClient);
    }

    /// <summary>
    /// Creates a RestClient that uses a NetRecordHttpClient from the current configuration
    /// </summary>
    /// <param name="baseAddress">The base address to set within the httpClient</param>
    /// <returns>A newly created NetRecordHttpClient</returns>
    public static RestClient CreateRestClient(
        this INetRecordConfiguration configuration,
        Uri baseAddress
    )
    {
        var httpClient = NetRecordHttpClient.CreateFromConfiguration(baseAddress, configuration);
        return new RestClient(httpClient);
    }

    /// <summary>
    /// Creates a RestClient that uses a NetRecordHttpClient from the current configuration
    /// </summary>
    /// <returns>A newly created NetRecordHttpClient</returns>
    public static RestClient CreateRestClient(this INetRecordConfiguration configuration)
    {
        var httpClient = NetRecordHttpClient.CreateFromConfiguration(
            (NetRecordConfiguration)configuration
        );
        return new RestClient(httpClient);
    }
}
