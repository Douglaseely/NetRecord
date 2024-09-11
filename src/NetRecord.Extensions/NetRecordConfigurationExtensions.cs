using NetRecord.Interfaces;
using NetRecord.Services;

namespace NetRecord.Extensions;

public static class NetRecordConfigurationExtensions
{
    /// <summary>
    /// Creates a NetRecordHttpClient from the current configuration
    /// </summary>
    /// <param name="baseAddress">The base address to set within the httpClient</param>
    /// <returns>A newly created NetRecordHttpClient</returns>
    public static NetRecordHttpClient CreateHttpClient(
        this NetRecordConfiguration configuration,
        Uri baseAddress
    )
    {
        return NetRecordHttpClient.CreateFromConfiguration(baseAddress, configuration);
    }

    /// <summary>
    /// Creates a NetRecordHttpClient from the current configuration
    /// </summary>
    /// <returns>A newly created NetRecordHttpClient</returns>
    public static NetRecordHttpClient CreateHttpClient(this NetRecordConfiguration configuration)
    {
        return NetRecordHttpClient.CreateFromConfiguration(configuration);
    }

    /// <summary>
    /// Creates a NetRecordHttpClient from the current configuration
    /// </summary>
    /// <param name="baseAddress">The base address to set within the httpClient</param>
    /// <returns>A newly created NetRecordHttpClient</returns>
    public static NetRecordHttpClient CreateHttpClient(
        this INetRecordConfiguration configuration,
        Uri baseAddress
    )
    {
        return NetRecordHttpClient.CreateFromConfiguration(baseAddress, configuration);
    }

    /// <summary>
    /// Creates a NetRecordHttpClient from the current configuration
    /// </summary>
    /// <returns>A newly created NetRecordHttpClient</returns>
    public static NetRecordHttpClient CreateHttpClient(this INetRecordConfiguration configuration)
    {
        return NetRecordHttpClient.CreateFromConfiguration((NetRecordConfiguration)configuration);
    }
}
