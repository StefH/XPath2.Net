using System;
using System.Globalization;
using System.IO;
using System.Threading;
using XPath2.TestRunner;

namespace XQTSRunConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CultureInfo.InvariantCulture = {0}", CultureInfo.InvariantCulture);
            Console.WriteLine("CultureInfo.InvariantCulture.Name = {0}", CultureInfo.InvariantCulture.Name);
            Console.WriteLine("CultureInfo.InvariantCulture.CultureTypes = {0}", CultureInfo.InvariantCulture.CultureTypes);
            Console.WriteLine("CultureInfo.InvariantCulture.DisplayName = {0}", CultureInfo.InvariantCulture.DisplayName);
            Console.WriteLine("CultureInfo.InvariantCulture.TwoLetterISOLanguageName = {0}", CultureInfo.InvariantCulture.TwoLetterISOLanguageName);
            Console.WriteLine("CultureInfo.InvariantCulture.ThreeLetterISOLanguageName = {0}", CultureInfo.InvariantCulture.ThreeLetterISOLanguageName);

            Console.WriteLine("CurrentCulture   = {0}", Thread.CurrentThread.CurrentCulture);
            Console.WriteLine("CurrentUICulture = {0}", Thread.CurrentThread.CurrentUICulture);
            var kelvinSign = "K";
            Console.WriteLine("{0} - ToLower={1} - ToLowerInvariant={2}", kelvinSign, kelvinSign.ToLower() == "k", kelvinSign.ToLowerInvariant() == "k");

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