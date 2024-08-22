using System.Diagnostics;
using NetRecord.Services;
using NetRecord.Utils;
using NetRecord.Utils.Enums;
using NetRecord.Utils.Exceptions;
using NetRecord.Utils.Models;

namespace NetRecord;

public class NetRecordHandler : DelegatingHandler
{
    private NetRecordConfiguration _configuration;

    internal NetRecordHandler(NetRecordConfiguration configuration)
    {
        _configuration = configuration;

        // In case we do make a real request, we need an inner handler
        InnerHandler = new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var stopwatch = new Stopwatch();
        switch (_configuration.Mode)
        {
            case ServiceMode.Record:
                stopwatch.Start();
                var recordResponse = await base.SendAsync(request, cancellationToken);
                stopwatch.Stop();
                await Recorder.Record(request, recordResponse, stopwatch.Elapsed, _configuration);
                return recordResponse;

            case ServiceMode.Replay:
                return await RecordPlayer.Replay(request, _configuration);

            case ServiceMode.Auto:
                var matchingTransaction = await RecordPlayer.CheckRequestForRecording(request, _configuration);

                if (matchingTransaction is not null)
                    return await RecordPlayer.ReplayRecording(matchingTransaction, request);

                stopwatch.Start();
                var autoResponse = await base.SendAsync(request, cancellationToken);
                stopwatch.Stop();

                await Recorder.Record(request, autoResponse, stopwatch.Elapsed, _configuration);
                
                return autoResponse;

            case ServiceMode.Bypass:
                var response = await base.SendAsync(request, cancellationToken);
                return response;

            default:
                throw new NetRecordException(
                    "Invalid ServiceMode when attempting to check request"
                );
        }
    }
}
