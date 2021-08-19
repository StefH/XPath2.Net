using System;
using System.IO;
using System.Net.Http;
using System.Xml;

namespace XPath2.TestRunner.FileResolvers
{
    public class OnlineFileResolver : FileResolverBase, IFileResolver
    {
        private readonly string _basePath;
        private readonly HttpClient _http = new HttpClient();

        public OnlineFileResolver(TextWriter tw, string uri, XmlNamespaceManager namespaceManager) : base(tw, uri, namespaceManager)
        {
            _basePath = new Uri(new Uri(uri), ".").OriginalString;
        }

        public string ResolveFileName(string nodeFilename, string type)
        {
            return $"{_basePath}/{nodeFilename}";
        }

        public string ResolveFileNameWithQueryExtension(string nodeFilename, string type)
        {
            return ResolveFileName(nodeFilename + _queryFileExtension, type);
        }

        public string GetResultAsString(XmlElement node, string fileName)
        {
            var path = $"{_basePath}{_resultOffsetPath + node.GetAttribute("FilePath")}{fileName})";

            return ReadAsString(path);
        }

        public string ReadAsString(XmlElement node)
        {
            var queryName = node.SelectSingleNode("ts:query/@name", _namespaceManager);
            var path = $"{_basePath}{_queryOffsetPath}{node.GetAttribute("FilePath")}{queryName.Value}{_queryFileExtension}";

            return ReadAsString(path);
        }

        private string ReadAsString(string uri)
        {
            return _http.GetAsync(uri).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }
    }
}