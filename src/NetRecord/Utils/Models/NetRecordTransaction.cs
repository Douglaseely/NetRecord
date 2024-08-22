using System.Text.Json.Serialization;
using NetRecord.Services;

namespace NetRecord.Utils.Models;

public class NetRecordTransaction
{
    [JsonPropertyName("Request")]
    public NetRecordRequest Request { get; set; }

    [JsonPropertyName("Response")]
    public NetRecordResponse Response { get; set; }

    [JsonPropertyName("RecordedAt")]
    public DateTime RecordedAt { get; set; }

    [JsonPropertyName("ElapsedTime")]
    public TimeSpan ElapsedTime { get; set; }

    internal bool CheckIfMatch(
        NetRecordTransaction transaction,
        NetRecordConfiguration configuration
    )
    {
        var request = transaction.Request;

        // First thing ever checked will always be the requests content type
        if (request.BodyContentType != Request.BodyContentType)
        {
            return false;
        }

        foreach (var identifierFunc in configuration.UniqueIdentifiers)
        {
            var thisValue = identifierFunc.Invoke(Request);
            var transactionValue = identifierFunc.Invoke(request);
            // This is a weird bit that has to be done because comparison between two strings cast to objects doesn't working with ==
            if (
                (thisValue is null && thisValue != transactionValue)
                || !(thisValue?.Equals(transactionValue) ?? true)
            )
            {
                return false;
            }
        }

        return true;
    }

    [JsonConstructor]
    internal NetRecordTransaction() { }
}
