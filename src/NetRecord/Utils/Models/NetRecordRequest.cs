using System.Text.Json.Serialization;
using NetRecord.Utils.Enums;
using NetRecord.Utils.Extensions;

namespace NetRecord.Utils.Models;

public class NetRecordRequest : NetRecordElement
{
    [JsonPropertyName("Method")]
    public HttpMethod Method { get; set; }

    [JsonPropertyName("Body")]
    public string? Body { get; set; }
    
    [JsonIgnore]
    internal RequestBodyContentType? BodyContentType
    {
        get => ContentTypeExtensions.FromString(BodyContentTypeString);
        set => BodyContentTypeString = value?.ToString();
    }
    
    [JsonPropertyName("ContentHeaders")]
    public IDictionary<string, string>? ContentHeaders { get; set; }
    
    [JsonPropertyName("RequestHeaders")]
    public IDictionary<string, string> RequestHeaders { get; set; }
    
    [JsonPropertyName("Uri")]
    public string? Uri { get; set; }

    /// <summary>
    /// This is easily the most important part of the request, the bodyContentType is the
    /// only part of the request that ALWAYS acts as a unique identifier token,
    /// as if its different the recording cannot be parsed
    /// </summary>
    [JsonPropertyName("BodyContentType")]
    public string? BodyContentTypeString { get; set; }
    
    [JsonConstructor]
    internal NetRecordRequest()
    {
        
    }
}