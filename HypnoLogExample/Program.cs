using System;
using System.Linq;
using HypnoLogLib;

namespace HypnoLogExample
{
    public class Program
    {
        private static void Main(string[] args)
        {
            // Run basic HypnoLog usage example
            BasicExample.Run();

            // if CLI argument "a" was given,
            // Run Advanced HypnoLog usage examples
            // (And some experimental stuff)
            if (args.Length >= 1 && args[0] == "a")
                AdvancedExample.Run();

        }
    }
}
