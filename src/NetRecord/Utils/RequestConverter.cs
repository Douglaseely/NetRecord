using System.Net.Http.Headers;
using NetRecord.Utils.Extensions;
using NetRecord.Utils.Models;

namespace NetRecord.Utils;

internal static class RequestConverter
{
    public static async Task<NetRecordRequest> ToRequestAsync(HttpRequestMessage requestMessage, RequestCensors requestCensors)
    {
        var requestBody = await ConverterHelpers.ConvertToStringAsync(requestMessage.Content);
        var request = new NetRecordRequest
        {
            Method = requestMessage.Method,
            Uri = requestCensors.ApplyUrlCensors(requestMessage.RequestUri?.AbsoluteUri),
            RequestHeaders = requestCensors.ApplyHeaderCensors(ConverterHelpers.ConvertToHeaders(requestMessage.Headers)),
            ContentHeaders = requestCensors.ApplyHeaderCensors(ConverterHelpers.ConvertToContentHeaders(requestMessage.Content)),
            BodyContentType = ContentTypeExtensions.DetermineContentType(requestBody)
        };
        request.Body = requestCensors.ApplyBodyParametersCensors(requestBody, request.BodyContentType);
        return request;
    }
    
    
}