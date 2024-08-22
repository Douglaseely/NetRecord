namespace NetRecord.Interfaces;

public interface INetRecordClientOptions
{
    public string ClientName { get; set; }
    public Uri BaseAddress { get; set; }
}
