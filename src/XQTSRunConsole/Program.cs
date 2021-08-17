using System;
using System.IO;
using XPath2.TestRunner;

namespace XQTSRunConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var errorWriter = args.Length > 1 ? TextWriter.Synchronized(new StreamWriter(args[1])) : null; // Needs to be Synchronized
            var runner = new XQTSRunner(Console.Out, errorWriter);

            var result = runner.RunParallel(args[0]);
            Console.WriteLine("{0} / {1} = {2}%", result.Passed, result.Total, result.Percentage);
        }
    }
}