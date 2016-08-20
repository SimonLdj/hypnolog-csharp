using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDebugLib;

namespace VDebugExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start example");
            VDebug.Init();

            // Example for simple string
            VDebug.Log("Example for logging string from C#");

            // Example for logging object
            VDebug.Log(new
            {
                Name = "Example Log from C# application",
                Value = new
                {
                    VarA = 4,
                    varB = 9
                }
            });

            // Example for logging array
            Random randNum = new Random();
            int[] exampleArray = Enumerable.Repeat(0, 50).Select(i => randNum.Next(0, 100)).ToArray();
            VDebug.Log(new
            {
                type = "array",
                name = "Example array from C#",
                array = exampleArray,
            });


        }
    }
}
