using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json.Serialization;
using NetRecord.Utils.Consts;
using NetRecord.Utils.Enums;
using NetRecord.Utils.Extensions;

namespace NetRecord.Utils.Models;

internal class NetRecordResponse : NetRecordElement
{
        [JsonPropertyName("Body")]
        internal string? Body { get; set; }

        [JsonIgnore]
        internal RequestBodyContentType? BodyContentType
        {
            get => ContentTypeExtensions.FromString(BodyContentTypeString);
            set => BodyContentTypeString = value?.ToString();
        }

       
        [JsonPropertyName("ContentHeaders")]
        internal IDictionary<string, string>? ContentHeaders { get; set; }
        
        [JsonPropertyName("HttpVersion")]
        internal Version HttpVersion { get; set; }
        
        [JsonPropertyName("ResponseHeaders")]
        internal IDictionary<string, string>? ResponseHeaders { get; set; }
        
        [JsonPropertyName("StatusCode")]
        internal HttpStatusCode? StatusCode { get; set; }
        
        [JsonPropertyName("StatusMessage")]
        internal string? StatusMessage { get; set; }
        
        [JsonPropertyName("BodyContentType")]
        private string? BodyContentTypeString { get; set; }

        internal HttpResponseMessage ToHttpResponseMessage(HttpRequestMessage requestMessage)
        {
            var result = new HttpResponseMessage(StatusCode!.Value);
            result.ReasonPhrase = StatusMessage;
            result.Version = HttpVersion;
            
            foreach (var header in ResponseHeaders ?? new Dictionary<string, string>()) 
                result.Headers.TryAddWithoutValidation(header.Key, header.Value.ToString());

            foreach (var header in StaticDefaults.ReplayHeaders) 
                result.Headers.TryAddWithoutValidation(header.Key, header.Value.ToString());

            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(Body ?? string.Empty));
            
            foreach (var header in ContentHeaders ?? new Dictionary<string, string>()) 
                content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToString());

            result.Content = content;
            result.RequestMessage = requestMessage;
            return result;
        }
}