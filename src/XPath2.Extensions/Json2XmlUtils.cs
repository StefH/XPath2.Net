using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Wmhelp.XPath2.Extensions;

public static class Json2XmlUtils
{
    private const string DefaultDynamicArray = "DynamicArray";
    private const string DefaultDynamicObject = "DynamicObject";

    /// <summary>
    /// Converts Json string to a XmlNode.
    /// </summary>
    /// <param name="json">The json.</param>
    /// <param name="rootObjectName">Name of the root object (default null).</param>
    /// <returns>XmlNode</returns>
    public static XmlNode? Json2XmlNode(string json, string? rootObjectName = null)
    {
        return Json2XmlNode(json, out _, rootObjectName);
    }

    /// <summary>
    /// Converts Json string to a XmlNode.
    /// </summary>
    /// <param name="json">The json.</param>
    /// <param name="dynamicRootObject">The value for the dynamicRootObject which is used.</param>
    /// <param name="rootObjectName">Name of the root object (default null).</param>
    /// <returns>XmlNode</returns>
    public static XmlNode? Json2XmlNode(string json, out string? dynamicRootObject, string? rootObjectName = null)
    {
        dynamicRootObject = null;
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        // Trim json string
        json = json.Trim();

        // In case the json response is an array, wrap the array in a fake RootObject to avoid
        // the exception : XmlNodeConverter can only convert JSON that begins with an object.
        if (json.StartsWith("[") && json.EndsWith("]"))
        {
            json = $"{{\"{DefaultDynamicObject}\":{json}}}";

            if (string.IsNullOrEmpty(rootObjectName))
            {
                rootObjectName = DefaultDynamicArray;
                dynamicRootObject = rootObjectName;
            }
        }

        // Try to convert the Json to a XmlNode
        if (TryDeserializeJsonToXmlNode(json, rootObjectName, out var node))
        {
            return node;
        }

        // If there is an error like : "JSON root object has multiple properties.", use a default rootname
        if (string.IsNullOrEmpty(rootObjectName) && TryDeserializeJsonToXmlNode(json, DefaultDynamicObject, out node))
        {
            dynamicRootObject = DefaultDynamicObject;
            return node;
        }

        // If this also fails, just return null
        return null;
    }

    /// <summary>
    /// Tries the deserialize json to XmlNode.
    /// </summary>
    /// <param name="json">The json.</param>
    /// <param name="rootObjectName">Name of the root object.</param>
    /// <param name="node">The node.</param>
    /// <returns>true if success, else false</returns>
    public static bool TryDeserializeJsonToXmlNode(string json, string? rootObjectName, [NotNullWhen(true)] out XmlNode? node)
    {
        node = null;

        try
        {
            // If rootObjectName is specified, use that name. Else use no rootObjectName
            node = !string.IsNullOrEmpty(rootObjectName) ? DeserializeXmlNode(json, rootObjectName) : DeserializeXmlNode(json);
        }
        catch
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Deserializes the XmlNode from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName"/>.
    /// Note that DateParseHandling is set to DateParseHandling.None to avoid issues with DateTime strings when converting json to xml.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <returns>The deserialized XmlNode</returns>
    private static XmlDocument DeserializeXmlNode(string value, string? deserializeRootElementName = null)
    {
        var nodeConvertor = new XmlNodeConverter
        {
            DeserializeRootElementName = deserializeRootElementName,
            WriteArrayAttribute = false
        };

        var serializerSettings = new JsonSerializerSettings
        {
            Converters = new[]
            {
                (JsonConverter) nodeConvertor
            },
            DateParseHandling = DateParseHandling.None
        };

        return (XmlDocument)JsonConvert.DeserializeObject(value, typeof(XmlDocument), serializerSettings)!;
    }
}