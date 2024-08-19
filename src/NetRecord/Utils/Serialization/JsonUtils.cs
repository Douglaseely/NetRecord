using System.Text.Json;
using System.Text.Json.Serialization;
using NetRecord.Utils.Exceptions;

namespace NetRecord.Utils.Serialization;

internal static class JsonUtils
{
    public static JsonSerializerOptions DefaultJsonOptions => new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };
    
    public static TOut DeserializeJsonToObject<TOut>(string? data, JsonSerializerOptions? options = null)
    {
        if (data is null || string.IsNullOrWhiteSpace(data)) throw new NetRecordException("No data exists to deserialize");
        try
        {
            options ??= DefaultJsonOptions; 
            return JsonSerializer.Deserialize<TOut>(data, options) ??
                   throw new NetRecordException("Deserialization failed");
        }
        catch (Exception e)
        {
            throw new NetRecordException("Deserialization failed", e);
        }
    }

    public static string SerializeObjectToJson(object? data, JsonSerializerOptions? options = null)
    {
        if (data is null) throw new NetRecordException("Cannot serailize null object");
        try
        {
            options ??= DefaultJsonOptions;
            return JsonSerializer.Serialize(data, options) ?? throw new NetRecordException("Serialization Failed");
        }
        catch (Exception e)
        {
            throw new NetRecordException("Serialization failed", e);
        }
    }
}