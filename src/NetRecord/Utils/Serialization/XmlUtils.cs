using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NetRecord.Utils.Exceptions;

namespace NetRecord.Utils.Serialization;

public static class XmlUtils
{
    public static TOut DeserializeXMLToObject<TOut>(string? data)
    {
        if (string.IsNullOrEmpty(data))
            throw new NetRecordException("No Xml data to deserialize");

        var serializer = new XmlSerializer(typeof(TOut));
        using var stringReader = new StringReader(data);
        using var xmlReader = new XmlTextReader(stringReader);
        return (TOut)(
            serializer.Deserialize(xmlReader)
            ?? throw new NetRecordException("Error deserializing XML")
        );
    }

    public static string SerializeToXML<TIn>(TIn? data)
    {
        var serializer = new XmlSerializer(typeof(TIn));
        using var memoryStream = new MemoryStream();
        using var xmlWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        serializer.Serialize(xmlWriter, data);
        memoryStream.Close();
        xmlWriter.Close();
        var xml = Encoding.UTF8.GetString(memoryStream.GetBuffer());
        // This is some magic substring stuff I don't actually understand that apparently converts the
        // memory buffer string to a straight xml string
        xml = xml.Substring(xml.IndexOf(Convert.ToChar(60)));
        return xml.Substring(0, (xml.LastIndexOf(Convert.ToChar(62)) + 1));
    }
}
