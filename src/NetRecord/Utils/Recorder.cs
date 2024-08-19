using NetRecord.Services;
using NetRecord.Utils.Models;

namespace NetRecord.Utils;

internal static class Recorder
{
    public static async Task<RecordFile> Record(HttpRequestMessage request, HttpResponseMessage? responseMessage, TimeSpan elapsedTime, NetRecordConfiguration configuration, RecordFile? recordFile = null)
    {
            var netRecordRequest = await RequestConverter.ToRequestAsync(request, configuration.RequestCensors);
            var netRecordResponse = await ResponseConverter.ToResponseAsync(responseMessage, configuration.RequestCensors);
            var httpTransaction = new NetRecordTransaction 
            {
                Request = netRecordRequest,
                Response = netRecordResponse,
                RecordedAt = DateTime.Now,
                ElapsedTime = elapsedTime 
            };
            
            recordFile ??= RecordFile.GetorCreateRecordFile(configuration);
            return recordFile.UpdateFileWithTransaction(httpTransaction, configuration);
    } 
}