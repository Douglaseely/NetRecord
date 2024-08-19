using NetRecord.Services;
using NetRecord.Utils;
using NetRecord.Utils.Enums;
using NetRecord.Utils.Models;

namespace NetRecord;

public class NetRecordHandler : DelegatingHandler
{
    private RecordFile _recordFile; 
    private NetRecordConfiguration _configuration;

    internal NetRecordHandler(NetRecordConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        switch (_configuration.Mode)
        {
            case ServiceMode.Record:
                stopwatch.Start();
                var recordResponse = await base.SendAsync(request, cancellationToken);
                stopwatch.Stop();
                await Recorder.Record(request, recordResponse, stopwatch.Elapsed, _configuration);
                return recordResponse;
            case ServiceMode.Replay:
                break;
            case ServiceMode.Auto:
                break;
            case ServiceMode.Bypass:
                break;
        }

        return new HttpResponseMessage();
    }

    
}