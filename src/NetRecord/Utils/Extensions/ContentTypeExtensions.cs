using NetRecord.Utils.Enums;
using NetRecord.Utils.Serialization;

namespace NetRecord.Utils.Extensions;

internal static class ContentTypeExtensions
{
    public static RequestBodyContentType? FromString(string? contentType)
    {
        if (contentType == null) return null;
        return contentType.ToLower() switch
        {
            "json" => RequestBodyContentType.Json,
            "xml" => RequestBodyContentType.Xml,
            "html" => RequestBodyContentType.Html,
            var _ => RequestBodyContentType.Text
        };
    }
    
    public static RequestBodyContentType DetermineContentType(string content)
    {
        if (IsJson(content))
        {
            return RequestBodyContentType.Json;
        }

        if (IsXml(content))
        {
            return RequestBodyContentType.Xml;
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (IsHtml(content))
        {
            return RequestBodyContentType.Html;
        }

        return RequestBodyContentType.Text;
    }
    
    private static bool IsHtml(string content)
    {
        return content.Contains("<html", StringComparison.CurrentCultureIgnoreCase);
    }

    private static bool IsJson(string content)
    {
        try
        {
            // try to serialize the string as JSON to an object
            JsonUtils.DeserializeJsonToObject<object>(content);
            return true;
        }
        catch (Exception)
        {
            // if it fails, it's not JSON
            return false;
        }
    }

    private static bool IsXml(string content)
    {
        try
        {
            // try to serialize the string as XML to an object
            XmlUtils.DeserializeXMLToObject<object>(content);
            return true;
        }
        catch (Exception)
        {
            // if it fails, it's not XML
            return false;
        }
    }
}