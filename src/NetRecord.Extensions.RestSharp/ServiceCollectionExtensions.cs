using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetRecord.Services;
using RestSharp;

namespace NetRecord.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Creates and adds an IRestClient to the IServiceCollection that utilizes the passed NetRecordHttpClient
    /// </summary>
    /// <param name="httpClient">The HttpClient that the RestClient will have injected into it</param>
    /// <param name="options">Optional RestClientOptions that will be passed into the RestClient</param>
    public static IServiceCollection AddNetRecordRestClient(
        this IServiceCollection services,
        NetRecordHttpClient httpClient,
        RestClientOptions? options = null
    )
    {
        var restClient = new RestClient(httpClient, options);
        services.TryAddSingleton<IRestClient>(restClient);

        return services;
    }

    /// <summary>
    /// Creates and add an IRestClient to the IServiceCollection, after creating a new NetRecordHttpClient using the passed Uri and configuration
    /// </summary>
    /// <param name="baseAddress">The Uri to use for the HttpClient</param>
    /// <param name="configuration">The configuration settings that will be injected into the HttpClient</param>
    /// <param name="options">Optional RestClientOptions that will be passed into the RestClient</param>
    /// <returns></returns>
    public static IServiceCollection AddNetRecordRestClient(
        this IServiceCollection services,
        Uri baseAddress,
        NetRecordConfiguration configuration,
        RestClientOptions? options = null
    )
    {
        var httpClient = NetRecordHttpClient.CreateFromConfiguration(baseAddress, configuration);
        httpClient.BaseAddress = baseAddress;

        var restClient = new RestClient(httpClient, options);
        services.TryAddSingleton<IRestClient>(restClient);

        return services;
    }

    /// <summary>
    /// Creates and add an IRestClient to the IServiceCollection, after creating a new NetRecordHttpClient using the passed Uri and configuration
    /// </summary>
    /// <param name="configuration">The configuration settings that will be injected into the HttpClient</param>
    /// <param name="options">Optional RestClientOptions that will be passed into the RestClient</param>
    /// <returns></returns>
    public static IServiceCollection AddNetRecordRestClient(
        this IServiceCollection services,
        NetRecordConfiguration configuration,
        RestClientOptions? options = null
    )
    {
        var httpClient = NetRecordHttpClient.CreateFromConfiguration(configuration);

        var restClient = new RestClient(httpClient, options);
        services.TryAddSingleton<IRestClient>(restClient);

        return services;
    }
}
