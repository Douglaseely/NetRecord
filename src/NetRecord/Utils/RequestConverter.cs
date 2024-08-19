using System.Net.Http.Headers;
using NetRecord.Utils.Extensions;
using NetRecord.Utils.Models;

namespace NetRecord.Utils;

internal static class RequestConverter
{
    public static async Task<NetRecordRequest> ToRequestAsync(HttpRequestMessage requestMessage, RequestCensors requestCensors)
    {
        var requestBody = await ToStringAsync(requestMessage.Content);
        var request = new NetRecordRequest
        {
            Method = requestMessage.Method,
            Uri = requestCensors.ApplyUrlCensors(requestMessage.RequestUri?.AbsoluteUri),
            RequestHeaders = requestCensors.ApplyHeaderCensors(ToHeaders(requestMessage.Headers)),
            ContentHeaders = requestCensors.ApplyHeaderCensors(ToContentHeaders(requestMessage.Content)),
            BodyContentType = ContentTypeExtensions.DetermineContentType(requestBody)
        };
        request.Body = requestCensors.ApplyBodyParametersCensors(requestBody, request.BodyContentType);
        return request;
    }
    
    public static async Task<string> ToStringAsync(HttpContent? content)
    {
        return content == null ? string.Empty : await content.ReadAsStringAsync();
    }
    
    public static IDictionary<string, string> ToHeaders(HttpHeaders headers)
    {
        IDictionary<string, string> dict = new Dictionary<string, string>();
        foreach (var h in headers) dict.Add(h.Key, string.Join(",", h.Value));
        return dict;
    }
    
    public static IDictionary<string, string> ToContentHeaders(HttpContent? content)
    {
        return content == null ? new Dictionary<string, string>() : ToHeaders(content.Headers);
    }
}