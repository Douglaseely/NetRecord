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
                // TODO: REPLAY
                return new HttpResponseMessage();
            
            case ServiceMode.Auto:
                // TODO: REPLAY NEEDED BEFORE MORE WORK
                var recordFile = RecordFile.GetorCreateRecordFile(_configuration);
                // if (recordFile.Recordings.Any())
                return new HttpResponseMessage();
            
            case ServiceMode.Bypass:
                var response = await base.SendAsync(request, cancellationToken);
                return response;
            
            default:
                throw new NetRecordException("Invalid ServiceMode when attempting to check request");
        }
    }
}