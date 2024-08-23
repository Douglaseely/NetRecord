using System.Text.Json;
using NetRecord.Utils.Extensions;
using NetRecord.Utils.Models;

namespace NetRecord.Utils;

internal static class RequestConverter
{
    public static async Task<NetRecordRequest> ToRequestAsync(
        HttpRequestMessage requestMessage,
        RequestCensors requestCensors,
        JsonSerializerOptions options
    )
    {
        var requestBody = await ConverterHelpers.ConvertToStringAsync(requestMessage.Content);
        var request = new NetRecordRequest
        {
            Method = requestMessage.Method,
            Uri = requestCensors.ApplyUrlCensors(requestMessage.RequestUri?.AbsoluteUri),
            RequestHeaders = requestCensors.ApplyHeaderCensors(
                ConverterHelpers.ConvertToHeaders(requestMessage.Headers)
            ),
            ContentHeaders = requestCensors.ApplyHeaderCensors(
                ConverterHelpers.ConvertToContentHeaders(requestMessage.Content)
            ),
            BodyContentType = ContentTypeExtensions.DetermineContentType(requestBody, options),
        };
        request.Body = requestCensors.ApplyBodyParametersCensors(
            requestBody,
            request.BodyContentType,
            options
        );
        return request;
    }
}
