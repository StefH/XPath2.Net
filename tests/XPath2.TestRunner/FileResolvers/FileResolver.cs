using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Schema;

namespace XPath2.TestRunner.FileResolvers
{
    public class LocalFileResolver : IFileResolver
    {
        private readonly TextWriter _out;
        private readonly string _queryOffsetPath;
        private readonly string _resultOffsetPath;
        private readonly string _queryFileExtension;
        private readonly XmlNamespaceManager _namespaceManager;
        private readonly string _basePath;

        public XmlDocument Catalog { get; }

        public LocalFileResolver(
            TextWriter tw,
            string fileName,
            XmlNamespaceManager namespaceManager)
        {
            _out = tw;
            _namespaceManager = namespaceManager;

            var schemaSet = new XmlSchemaSet();
            var settings = new XmlReaderSettings
            {
                Schemas = schemaSet,
                DtdProcessing = DtdProcessing.Ignore
            };
            var resolver = new XmlUrlResolver
            {
                Credentials = CredentialCache.DefaultCredentials
            };

            settings.XmlResolver = resolver;
            settings.NameTable = namespaceManager.NameTable;
            settings.ValidationFlags = XmlSchemaValidationFlags.ProcessSchemaLocation | XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationType = ValidationType.Schema;

            Catalog = new XmlDocument(namespaceManager.NameTable);

            using (var reader = XmlReader.Create(fileName, settings))
            {
                Catalog.Load(reader);
                reader.Close();
            }

            _queryOffsetPath = Catalog.DocumentElement.GetAttribute("XQueryQueryOffsetPath");
            _resultOffsetPath = Catalog.DocumentElement.GetAttribute("ResultOffsetPath");
            _queryFileExtension = Catalog.DocumentElement.GetAttribute("XQueryFileExtension");

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