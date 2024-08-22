using System.Net.Http.Headers;

namespace NetRecord.Utils;

public static class ConverterHelpers
{
    public static async Task<string> ConvertToStringAsync(HttpContent? content)
    {
        return content == null ? string.Empty : await content.ReadAsStringAsync();
    }

    public static IDictionary<string, string> ConvertToHeaders(HttpHeaders headers)
    {
        IDictionary<string, string> dict = new Dictionary<string, string>();
        foreach (var h in headers)
            dict.Add(h.Key, string.Join(",", h.Value));
        return dict;
    }

    public static IDictionary<string, string> ConvertToContentHeaders(HttpContent? content)
    {
        return content == null
            ? new Dictionary<string, string>()
            : ConvertToHeaders(content.Headers);
    }
}
