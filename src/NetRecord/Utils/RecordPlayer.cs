using NetRecord.Services;
using NetRecord.Utils.Exceptions;
using NetRecord.Utils.Models;

namespace NetRecord.Utils;

internal static class RecordPlayer
{
    public static async Task<NetRecordTransaction?> CheckRequestForRecording(
        HttpRequestMessage request,
        NetRecordConfiguration configuration
    )
    {
        var netRecordRequest = await RequestConverter.ToRequestAsync(
            request,
            configuration.RequestCensors,
            configuration.JsonSerializerOptions
        );

        var transaction = NetRecordTransaction.FromRequest(netRecordRequest);

        var recordFile = RecordFile.GetFile(configuration, transaction);

        return recordFile?.GetMatchingTransaction(configuration, transaction);
    }

    public static async Task<HttpResponseMessage> Replay(
        HttpRequestMessage httpRequest,
        NetRecordConfiguration configuration
    )
    {
        var matchingTransaction = await CheckRequestForRecording(httpRequest, configuration);

        if (matchingTransaction is null)
            throw new NetRecordException(
                $"Could not find matching request for {httpRequest.RequestUri}"
            );

        return await ReplayRecording(matchingTransaction, httpRequest);
    }

    public static async Task<HttpResponseMessage> ReplayRecording(
        NetRecordTransaction transaction,
        HttpRequestMessage httpRequest
    )
    {
        return transaction.Response.ToHttpResponseMessage(httpRequest);
    }
}
