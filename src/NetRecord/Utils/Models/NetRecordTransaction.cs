using NetRecord.Services;

namespace NetRecord.Utils.Models;

internal class NetRecordTransaction
{
    public NetRecordRequest Request;
    public NetRecordResponse Response;
    public DateTime RecordedAt;
    public TimeSpan ElapsedTime;

    internal bool CheckIfMatch(NetRecordTransaction transaction, NetRecordConfiguration configuration)
    {
        var request = transaction.Request;
        
        // First thing ever checked will always be the requests content type
        if (request.BodyContentType != Request.BodyContentType)
        {
            return false;
        }

        foreach (var identifierFunc in configuration.UniqueIdentifiers)
        {
            if (identifierFunc.Invoke(request) != identifierFunc.Invoke(Request))
            {
                return false;
            }
        }

        return true;
    }
}