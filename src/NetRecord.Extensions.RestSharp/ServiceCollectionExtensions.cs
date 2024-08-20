using Microsoft.Extensions.DependencyInjection;
using NetRecord.Services;
using RestSharp;

namespace NetRecord.Extensions.RestSharp;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name=""></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddNetRecordRestClient(NetRecordHttpClient httpClient, NetRecordConfiguration configuration, RestClientOptions? options = null)
    {
        
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration">The configuration used by the client</param>
    public static IServiceCollection AddNetRecordRestClient(Uri baseAddress, NetRecordConfiguration configuration, RestClientOptions? options = null)
    {
        var httpClient = NetRecordHttpClient.CreateFromConfiguration(configuration);
        httpClient.BaseAddress = baseAddress;
    }
}