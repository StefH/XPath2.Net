using System;
using System.IO;
using System.Xml;

namespace XPath2.TestRunner.FileResolvers
{
    public class LocalFileResolver : FileResolverBase, IFileResolver
    {
        private readonly string _basePath;

        public LocalFileResolver(TextWriter tw, string fileName, XmlNamespaceManager namespaceManager) : base(tw, fileName, namespaceManager)
        {
            _basePath = Path.GetDirectoryName(fileName);
        }

        public string ResolveFileName(string nodeFilename, string type)
        {
            string schemaFileName = Path.Combine(_basePath, nodeFilename).Replace('/', Path.DirectorySeparatorChar);
            if (!File.Exists(schemaFileName))
            {
                _out.WriteLine("{0} file {1} does not exists", type, schemaFileName);
            }

            return schemaFileName;
        }

        public string ResolveFileNameWithQueryExtension(string nodeFilename, string type)
        {
            return ResolveFileName(nodeFilename + _queryFileExtension, type);
        }

        public string GetResultAsString(XmlElement node, string fileName)
        {
            var path = Path.Combine(_basePath, (_resultOffsetPath + node.GetAttribute("FilePath") + fileName).Replace('/', Path.DirectorySeparatorChar));

            using (var textReader = new StreamReader(path, true))
            {
                return textReader.ReadToEnd();
            }
        }

        public string ReadAsString(XmlElement node)
        {
            var queryName = node.SelectSingleNode("ts:query/@name", _namespaceManager);
            var fileName = Path.Combine(_basePath, (_queryOffsetPath + node.GetAttribute("FilePath") + queryName.Value + _queryFileExtension).Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(fileName))
            {
                _out.WriteLine("File {0} not exists.", fileName);
                throw new ArgumentException();
            }

            using (var textReader = new StreamReader(fileName, true))
            {
                return textReader.ReadToEnd();
            }
        }
    }
}