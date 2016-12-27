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

            // Example of logging a string
            VDebug.Log("Example for logging string from C#");

            // Example of logging an integer
            VDebug.Log(758593);

            // Example of logging a double
            VDebug.Log(100.999);

            // Example of logging an Enum
            VDebug.Log(Colors.Green);

            // Example of logging array of numbers
            Random randNum = new Random();
            int[] exampleArray = Enumerable.Repeat(0, 50).Select(i => randNum.Next(0, 100)).ToArray();
            VDebug.Log(exampleArray);

            // Example of logging an object
            VDebug.Log(new Surfboard() {Name = "Lib Tech Bowl", Length = 6.2, Volume = 30.8});

            // Example of logging an anonymous object
            VDebug.Log(new
            {
                BrandName = "Seat",
                ModelName = "Mii",
                Engine = new {
                    NumberOfCylinders = 3,
                    Acceleration = 14.4
                },
                Color = Colors.Red
            });

        }

        private enum Colors
        {
            Red,
            Green,
            Blue
        }

        private class Surfboard
        {
            public string Name { get; set; }
            public double Length { get; set; }
            public double Volume { get; set; }
        }

    }
}
