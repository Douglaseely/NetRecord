
namespace NetRecord.Services;

public class NetRecordHttpClient : HttpClient
{
    internal NetRecordHttpClient(NetRecordHandler handler) : base(handler)
    {
        
    }
}