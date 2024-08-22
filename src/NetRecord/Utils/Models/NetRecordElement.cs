using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetRecord.Utils.Models;

public class NetRecordElement
{
    internal string ToJson(params JsonConverter[] converters)
    {
        if (this == null)
            throw new Exception("No object to serialize");

        // modify settings so elements will be ordered
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(this, options);
    }
}
