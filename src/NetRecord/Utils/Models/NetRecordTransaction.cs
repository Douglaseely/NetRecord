using NetRecord.Services;

namespace NetRecord.Utils.Models;

internal class NetRecordTransaction
{
    public NetRecordRequest Request;
    public NetRecordResponse Response;
    public DateTime RecordedAt;
    public TimeSpan ElapsedTime;
    
    internal bool CheckIfMatch(NetRecordTransaction request, NetRecordConfiguration configuration)
    {
        
    }
}