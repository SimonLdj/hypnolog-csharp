using System;
using System.Linq;
using VDebugLib;

namespace VDebugExample
{
    public class Program
    {

        private static void Main(string[] args)
        {
            Console.WriteLine("Start example");
            VDebug.Init();

            // Example of logging a string
            VDebug.Log("Example for logging string from C#");

            // Example of logging variable with its name
            // This is good to avoid code like this:
            // var x = GetSomeValue();
            // Log("x: " + x);
            // Then you change the name of 'x' and the logging become misleading.
            // Note: To log variable with it's name use the "NamedLog" function, and warp the
            // variable with new {} deceleration.
            var walter = "Also known as Mr.White";
            VDebug.NamedLog(new {walter});

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

            // Example of logging a custom object
            VDebug.Log(new Surfboard() {Name = "Lib Tech Bowl", Length = 6.2, Volume = 30.8});

            // Example of logging a custom object with name
            var mySurfBoard = new Surfboard() {Name = "Lib Tech Bowl", Length = 6.2, Volume = 30.8};
            VDebug.NamedLog(new {mySurfBoard});

            // Example of logging an anonymous object
            VDebug.Log(new
            {
                BrandName = "Seat",
                ModelName = "Mii",
                Engine = new
                {
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
