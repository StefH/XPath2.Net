using System;
using System.IO;
using System.Threading;
using XPath2.TestRunner;

namespace XQTSRunConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CurrentCulture   = {0}", Thread.CurrentThread.CurrentCulture);
            Console.WriteLine("CurrentUICulture = {0}", Thread.CurrentThread.CurrentUICulture);

            var passedWriter = args.Length > 1 ? TextWriter.Synchronized(new StreamWriter(args[1])) : null; // Needs to be Synchronized
            var errorWriter = args.Length > 2 ? TextWriter.Synchronized(new StreamWriter(args[2])) : null; // Needs to be Synchronized
            var runner = new XQTSRunner(Console.Out, passedWriter, errorWriter);

            //var result1 = runner.Run(args[0], RunType.Parallel);
            //Console.WriteLine("{0} / {1} = {2}%", result1.Passed, result1.Total, result1.Percentage);

            var result2 = runner.Run(args[0], RunType.Sequential);
            Console.WriteLine("{0} / {1} = {2}%", result2.Passed, result2.Total, result2.Percentage);

            passedWriter.Flush();
            passedWriter.Close();

            errorWriter.Flush();
            errorWriter.Close();
        }
    }
}