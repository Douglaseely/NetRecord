
namespace NetRecord.Services;

/// <summary>
/// 
/// </summary>
public class NetRecordHttpClient : HttpClient
{
    internal NetRecordHandler handler { get; }

    internal NetRecordHttpClient(NetRecordHandler handler) : base(handler)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static NetRecordHttpClient CreateFromConfiguration(Uri baseAddress, NetRecordConfiguration configuration)
    {
        var handler = new NetRecordHandler(configuration);
        var client = new NetRecordHttpClient(handler);
        client.BaseAddress = baseAddress;
        return client;
    }
}