using System.Xml;

namespace XPath2.TestRunner.FileResolvers
{
    public interface IFileResolver
    {
        XmlDocument Catalog { get; }

        string ReadAsString(XmlElement node);

        string GetResultAsString(XmlElement node, string fileName);

        string ResolveFileName(string nodeFilename, string type);

        string ResolveFileNameWithQueryExtension(string nodeFilename, string type);
    }
}