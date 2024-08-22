using NetRecord.Services;
using NetRecord.Utils.Models;

namespace NetRecord.Utils;

internal static class Recorder
{
    /// <summary>
    /// Record an http transaction, overwriting any pre-existing recording matching the uniqueness identifiers
    /// </summary>
    /// <param name="request">The Http Request to record</param>
    /// <param name="responseMessage">The Http Response to record</param>
    /// <param name="elapsedTime">How long the full request response cycle took</param>
    /// <param name="configuration">The NetRecordConfiguration used for all user settings</param>
    /// <param name="recordFile">This is expected to be the correct recordFile if passed</param>
    /// <returns>The RecordFile that is either passed, found, or created</returns>
    public static async Task<RecordFile> Record(
        HttpRequestMessage request,
        HttpResponseMessage? responseMessage,
        TimeSpan elapsedTime,
        NetRecordConfiguration configuration,
        RecordFile? recordFile = null
    )
    {
        var netRecordRequest = await RequestConverter.ToRequestAsync(
            request,
            configuration.RequestCensors,
            configuration.JsonSerializerOptions
        );
        var netRecordResponse = await ResponseConverter.ToResponseAsync(
            responseMessage,
            configuration.RequestCensors,
            configuration.JsonSerializerOptions
        );
        var httpTransaction = new NetRecordTransaction
        {
            Request = netRecordRequest,
            Response = netRecordResponse,
            RecordedAt = DateTime.Now,
            ElapsedTime = elapsedTime
        };

        recordFile ??= RecordFile.GetorCreateRecordFile(configuration, httpTransaction);
        return recordFile.UpdateFileWithTransaction(httpTransaction, configuration);
    }
}
