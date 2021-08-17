using System;

namespace XQTSRunConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var runner = new XQTSRunner(Console.Out);

            runner.Run(args[0]);
        }
    }
}
