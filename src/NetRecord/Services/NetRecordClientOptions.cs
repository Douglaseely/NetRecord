using NetRecord.Interfaces;

namespace NetRecord.Services;

public class NetRecordClientOptions(string clientName, Uri baseAddress) : INetRecordClientOptions
{
    public string ClientName { get; set; } = clientName;
    public Uri BaseAddress { get; set; } = baseAddress;
}
