using System;
using System.IO;
using XPath2.TestRunner;

namespace XQTSRunConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var passedWriter = args.Length > 1 ? TextWriter.Synchronized(new StreamWriter(args[1])) : null; // Needs to be Synchronized
            var errorWriter = args.Length > 2 ? TextWriter.Synchronized(new StreamWriter(args[2])) : null; // Needs to be Synchronized
            var runner = new XQTSRunner(Console.Out, passedWriter, errorWriter);

            //var result1 = runner.Run(args[0], RunType.Parallel);
            //Console.WriteLine("{0} / {1} = {2}%", result1.Passed, result1.Total, result1.Percentage);

            var result2 = runner.Run(args[0], RunType.Sequential);
            Console.WriteLine("{0} / {1} = {2}%", result2.Passed, result2.Total, result2.Percentage);
        }
    }
}