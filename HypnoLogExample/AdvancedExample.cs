using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Do this to easily use HypnoLogLib as HL in your code
using HL = HypnoLogLib.HypnoLog;
using HypnoLogLib;

namespace HypnoLogExample
{
    public static class AdvancedExample
    {
        public static void Run()
        {
            // TODO: complete/fix Advanced examples

            // HypnoLog C# usage examples:

            // Initialize the logger
            // It is not mandatory to call Initialize but it is recommended to do it as soon as
            // possible in the begging of the program. This will help marking the begging
            // of new session in a proper way.
            // If Initialize was not invoked manually, it will be called implicitly at the first
            // logging action.
            HL.Initialize(serverUri:"http://localhost:7000/");
            // Also you can call initialization without parameters. Default server will be used (http://localhost:7000/).

            // == Logging ==

            // Example of logging a string
            HL.Log("Example for logging string from C#");
            HL.Log("Example for logging string from C# version {0} at {1}", 4.5 , DateTime.Now);

            // Example of logging variable with its name
            //  This is good to avoid code like this:
            // 	    var x = GetSomeValue();
            // 		Log("x: " + x);
            //  Then you change the name of 'x' and the logging become misleading.
            // Note: To log variable with it's name use the "NamedLog" function, and warp the
            // variable with `new {}` deceleration.
            var walter = "Also known as Mr.White";
            HL.NamedLog(new {walter});

            // Example of logging an integer
            HL.Log(758593);

            // Example of logging a double
            HL.Log(100.999);

            // Example of logging an Enum
            HL.Log(Colors.Green);


            //// == Watching ==

            //// Example of watching a variable.
            //// Note: To watch variable we need it's name and therefor we have to warp
            //// the variable with new {} deceleration as we do in "NamedLog" function.
            //var sky = "Blue clear sky";
            //HL.Watch(new { sky });
            //// Change the value and watch it changing.
            //sky = "Gray foggy sky";
            //HL.Watch(new { sky });
            //// Example of watching two variables with the same name, in different scopes.
            //CheckTheWeather();

            // == Tags ==

            // Example of logging with tags.
            // Note: To log with tags you should add your tags with the 'Tag' method and then invoke logging method of your choice.
            //HL.Tag("#network").Log("Some network data Tagged as #network");
            //HL.Tag("weather").Log("Weather will be {0}. Tagged as #weather", "sunny");
            //var weatherOverNetwork =
            //    "Weather report received over network:\n" +
            //        "Weather will be cool and cloudy\n" +
            //        "Sun will be soft but warm\n" +
            //        "Waves will be head hight \\m/\n";
            // You can also tag with multiple tags, just separate them with space or hash
            //HL.Tag("#weather #network").NamedLog(new { weatherOverNetwork });

            // == Synchronous usage ==

            // In case you want to use some logging methods from scopes do not allow multi-threading
            // such as invoking method from Immediate Window in Visual Studio,
            // use the Synchronous version of the logging methods.

            //// Example of synchronous logging
            //HL.LogSync("Logging some string, synchronously!");
            //HL.Tag("sync").LogSync("Logging some string, synchronously! at {0}, tagged as #{1}", DateTime.Now, "sync");
            //var someVariable = "Which will be logged Synchronously";
            //HL.NamedLogSync(new {someVariable});
            //someVariable += " again, with a #sync tag";
            //HL.Tag("sync").NamedLogSync(new {someVariable});
        }

        /// <summary>
        /// Example of watching a variable in another scope
        /// </summary>
        private static void CheckTheWeather()
        {
            var sky = "Blood moon night";
            HL.Watch(new { sky });
        }

        private enum Colors
        {
            Red,
            Green,
            Blue
        }
    }
}
