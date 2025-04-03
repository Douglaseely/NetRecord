using NetRecord.Services;
using NetRecord.Utils.Exceptions;
using NetRecord.Utils.Models;

namespace NetRecord.Utils;

internal static class RecordPlayer
{
    public static async Task<NetRecordTransaction?> CheckRequestForRecording(
        NetRecordRequest request,
        NetRecordConfiguration configuration
    )
    {
        var transaction = NetRecordTransaction.FromRequest(request);

        var recordFile = RecordFile.GetFile(configuration, transaction);

        return recordFile?.GetMatchingTransaction(configuration, transaction);
    }

    public static async Task<HttpResponseMessage> Replay(
        NetRecordRequest request,
        HttpRequestMessage httpRequest,
        NetRecordConfiguration configuration
    )
    {
        var matchingTransaction = await CheckRequestForRecording(request, configuration);

        if (matchingTransaction is null)
        {
            var uniqueParameters = configuration
                .UniqueIdentifiers.Select(uniqueIdentifier => uniqueIdentifier.Invoke(request))
                .ToList();

            throw new NetRecordException(
                $"Could not find matching request for request to {request.Uri}, with parameters values: ({string.Join(", ", uniqueParameters)})."
            );
        }

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
