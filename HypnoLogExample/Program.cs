using System;
using System.Linq;
using HypnoLogLib;

namespace HypnoLogExample
{
    public class Program
    {
        private static void Main(string[] args)
        {
            // if CLI argument "a" was given
            if (args.Length >= 1 && args[0] == "a")
            {
                // Run Advanced HypnoLog usage examples
                // (And some experimental stuff)
                AdvancedExample.Run();
            }
            else
            {
                // Run basic HypnoLog usage example
                BasicExample.Run();
            }
        }
    }
}
