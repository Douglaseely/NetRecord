using NetRecord.Utils.Exceptions;
using NetRecord.Utils.Extensions;
using NetRecord.Utils.Models;

namespace NetRecord.Utils;

internal static class ResponseConverter
{
    public static async Task<NetRecordResponse> ToResponseAsync(HttpResponseMessage? responseMessage,
        RequestCensors censors)
    {
        if (responseMessage is null)
            throw new NetRecordException("HttpResponseMessage cannot be null");
        
        var responseBody = await ConverterHelpers.ConvertToStringAsync(responseMessage.Content);
        
        var response = new NetRecordResponse
        {
            StatusCode = responseMessage.StatusCode,
            StatusMessage = responseMessage.ReasonPhrase,
            ResponseHeaders = censors.ApplyHeaderCensors(ConverterHelpers.ConvertToHeaders(responseMessage.Headers)),
            ContentHeaders = censors.ApplyHeaderCensors(ConverterHelpers.ConvertToContentHeaders(responseMessage.Content)),
            BodyContentType = ContentTypeExtensions.DetermineContentType(responseBody),
            HttpVersion = responseMessage.Version
        };
        response.Body = censors.ApplyBodyParametersCensors(responseBody, response.BodyContentType);
        return response;
    }
    
}