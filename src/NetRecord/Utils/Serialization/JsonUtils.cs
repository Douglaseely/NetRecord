using System.Text.Json;
using NetRecord.Utils.Exceptions;
using NetRecord.Utils.Models;

namespace NetRecord.Utils.Serialization;

internal static class JsonUtils
{
    public static TOut DeserializeJsonToObject<TOut>(string? data, JsonSerializerOptions options)
    {
        if (data is null || string.IsNullOrWhiteSpace(data))
            throw new NetRecordException("No data exists to deserialize");
        try
        {
            return JsonSerializer.Deserialize<TOut>(data, options)
                ?? throw new NetRecordException("Deserialization failed");
        }
        catch (Exception e)
        {
            throw new NetRecordException("Deserialization failed", e);
        }
    }

    public static TOut? DeserializeNullableJsonToObject<TOut>(
        string? data,
        JsonSerializerOptions options
    )
        where TOut : class
    {
        if (data is null || string.IsNullOrWhiteSpace(data))
            return null;
        try
        {
            return JsonSerializer.Deserialize<TOut>(data, options)
                ?? throw new NetRecordException("Deserialization failed");
        }
        catch (Exception e)
        {
            throw new NetRecordException("Deserialization failed", e);
        }
    }

    public static string SerializeObjectToJson<TIn>(TIn? data, JsonSerializerOptions options)
    {
        if (data is null)
            throw new NetRecordException("Cannot serialize null object");
        try
        {
            var serializedResult =
                JsonSerializer.Serialize(data, options)
                ?? throw new NetRecordException("Serialization Failed");
            return serializedResult;
        }
        catch (Exception e)
        {
            throw new NetRecordException("Serialization failed", e);
        }
    }

    public static string SerializeObjectToJson(
        IEnumerable<NetRecordTransaction>? data,
        JsonSerializerOptions options
    )
    {
        if (data is null)
            throw new NetRecordException("Cannot serialize null object");
        try
        {
            var serializedResult =
                JsonSerializer.Serialize(data, options)
                ?? throw new NetRecordException("Serialization Failed");
            return serializedResult;
        }
        catch (Exception e)
        {
            throw new NetRecordException("Serialization failed", e);
        }
    }
}
