using System;
using System.Linq;
using HypnoLogLib;

namespace HypnoLogExample
{
    public class Program
    {

        private static void Main(string[] args)
        {
            Console.WriteLine("Open http://localhost:7000/ to see output");

            // Initialize the logger
            // It is not mandatory to call Initialize but it is recommended to do it as soon as
            // possible in the begging of the program. This will help marking the begging
            // of new session in a proper way.
            // If Initialize was not invoked manually, it will be called implicitly at the first
            // logging action.
            HypnoLog.Initialize("http://localhost:7000/");
            // Also you can call initialization without parameters. Default server will be used (http://localhost:7000/).

            // Example of logging a string
            HypnoLog.Log("Example for logging string from C#");

            // Example of logging variable with its name
            // This is good to avoid code like this:
            // var x = GetSomeValue();
            // Log("x: " + x);
            // Then you change the name of 'x' and the logging become misleading.
            // Note: To log variable with it's name use the "NamedLog" function, and warp the
            // variable with new {} deceleration.
            var walter = "Also known as Mr.White";
            HypnoLog.NamedLog(new {walter});

            // Example of logging an integer
            HypnoLog.Log(758593);

            // Example of logging a double
            HypnoLog.Log(100.999);

            // Example of logging an Enum
            HypnoLog.Log(Colors.Green);

            // Example of logging array of numbers
            Random randNum = new Random();
            int[] exampleArray = Enumerable.Repeat(0, 50).Select(i => randNum.Next(0, 100)).ToArray();
            HypnoLog.Log(exampleArray);

            // Example of logging a custom object
            HypnoLog.Log(new Surfboard() {Name = "Lib Tech Bowl", Length = 6.2, Volume = 30.8});

            // Example of logging a custom object with name
            var mySurfBoard = new Surfboard() {Name = "Lib Tech Bowl", Length = 6.2, Volume = 30.8};
            HypnoLog.NamedLog(new {mySurfBoard});

            // Example of logging an anonymous object
            HypnoLog.Log(new
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

            // Example of watching a variable.
            // Note: To watch variable we need it's name and therefor we have to warp
            // the variable with new {} deceleration as we do in "NamedLog" function.
            var sky = "Blue clear sky";
            HypnoLog.Watch(new { sky });
            // Change the value and watch it changing.
            sky = "Gray foggy sky";
            HypnoLog.Watch(new { sky });
            // Example of watching two variables with the same name, in different scopes.
            CheckTheWeather();

            //Example of logging with tags.
            // Note: To log with tags you should add your tags with the 'Tag' method and then invoke logging method of your choice.
            HypnoLog.Tag("#network").Log("Some network data Tagged as #network");
            HypnoLog.Tag("##weather").Log("Weather will be sunny");
            var weatherOverNetwork = "Weather received over net will be cool and cloudy";
            // You can also tag with multiple tags, just separate them with space or hash
            HypnoLog.Tag("#weather #network").NamedLog(new { weatherOverNetwork });
        }

        /// <summary>
        /// Example of watching a variable in another scope
        /// </summary>
        private static void CheckTheWeather()
        {
            var sky = "Blood moon night";
            HypnoLog.Watch(new { sky });
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
