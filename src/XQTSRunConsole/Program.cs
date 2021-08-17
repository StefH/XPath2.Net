using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Schema;

namespace XQTSRunConsole
{
    class Program
    {
        

        static void Main(string[] args)
        {
            var runner = new XQTSRunner(Console.Out);

            runner.OpenCatalog(@"C:\Users\azurestef\Downloads\XQTS_1_0_2\XQTSCatalog.xml");
        }

        
    }
}
