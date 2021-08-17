using System;
using XPath2.TestRunner;

namespace XQTSRunConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var runner = new XQTSRunner(Console.Out);

            var result = runner.Run(args[0]);
            Console.WriteLine("{0} / {1} = {2}%", result.Passed, result.Total, result.Percentage);
        }
    }
}
