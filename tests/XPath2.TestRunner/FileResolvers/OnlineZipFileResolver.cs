using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Xml;
using Ionic.Zip;

namespace XPath2.TestRunner.FileResolvers
{
    public class OnlineZipFileResolver : FileResolverBase, IFileResolver
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private readonly string _basePath;

        public OnlineZipFileResolver(TextWriter tw, string zipUriWithTempFolder, XmlNamespaceManager namespaceManager) : base(tw, DownloadAndUnzip(tw, zipUriWithTempFolder), namespaceManager)
        {
            var parts = zipUriWithTempFolder.Split('|');
            _basePath = Path.Combine(parts[1], "XQTS_1_0_2");
        }

        private static string DownloadAndUnzip(TextWriter tw, string zipUriWithTempFolder)
        {
            var parts = zipUriWithTempFolder.Split('|');
            var zipFile = Path.Combine(parts[1], "XQTS_1_0_2.zip");
            var extractLocation = Path.Combine(parts[1], "XQTS_1_0_2");
            var xmlFile = Path.Combine(extractLocation, "XQTSCatalog.xml");

            if (File.Exists(zipFile))
            {
                return xmlFile;
            }

            var sw = new Stopwatch();
            sw.Start();
            tw.WriteLine("DownloadFileFromInternet");
            DownloadFileFromInternet(parts[0], zipFile);
            sw.Stop();
            tw.WriteLine(sw.Elapsed);

            tw.WriteLine("ExtractToDirectory");
            sw.Start();
            using (var zip = ZipFile.Read(zipFile))
            {
                zip.ExtractAll(extractLocation, ExtractExistingFileAction.DoNotOverwrite);
            }
            sw.Stop();
            tw.WriteLine(sw.Elapsed);

            return xmlFile;
        }

        private static void DownloadFileFromInternet(string uri, string path)
        {
            using (Stream contentStream = HttpClient.GetAsync(uri).GetAwaiter().GetResult().Content.ReadAsStreamAsync().GetAwaiter().GetResult(),
                        stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                contentStream.CopyTo(stream);
            }
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
