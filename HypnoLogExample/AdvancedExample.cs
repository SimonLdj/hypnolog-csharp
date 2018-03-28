using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Do this to easily use HypnoLog as HL in your code
using HL = HypnoLogLib.HypnoLog;

namespace HypnoLogExample
{
    public static class AdvancedExample
    {
        public static void Run()
        {
            // HypnoLog C# Advanced usage examples:

            // == Initializing ==
            // Note: this is an experimental feature

            // Initialize the HypnoLog
            // It is not mandatory to call Initialize but it is recommended to do so as soon as
            // possible in the begging of the program. This will help marking the begging
            // of new session in a proper way, by logging "new session" message.
            // If Initialize was not invoked manually, it will be called implicitly at the first
            // logging action.
            HL.Initialize(shouldRedirect: true, serverUri:"http://localhost:7000/");
            // Also you can call initialization without parameters. Default server will be used (http://localhost:7000/).
            // Setting shouldRedirect to true, will redirect all System.Console output to HypnoLog.
            // This can be useful if u already have a program with a lot of output to the console.

            // == Logging ==
            // Note: this is an experimental feature

            // Example of logging a string with arguments which will be format,
            // as you would do in `Console.WriteLine("x = {0}", x);`
            HL.Log("This is logged form C# version {0} at {1}", Environment.Version , DateTime.Now);

            // TODO: when logging `HL.Log("bal is {0} ", args: "sunny");` the `args` is required
            // otherwise the method is ambiguous
            //HL.Log("bal is {0} ", "sunny");


            // == Tags ==
            // Note: this is an experimental feature

            // Example of logging with tags.
            // To log with tags, add your tags with the `Tag()` method and then invoke the logging method of your choice.
            // You can tag with multiple tags,
            // tags can be separated by hash (`#`) or space(` `).
            HL.Tag("#info").Log("Some data Tagged as info");
            HL.Tag("#info #weather").Log("Weather will be sunny. Tagged as #info and as #weather");
            var detailedWeatherDescription =
                "Weather report received over network:\n" +
                    "Weather will be cool and cloudy\n" +
                    "Sun will be soft but warm\n" +
                    "Waves will be head hight \\m/\n";
            // Combine tagging and named-logging
            HL.Tag("info weather detailed").NamedLog(new { detailedWeatherDescription });
            // Log some graph and tag it
            HL.Tag("#info #numbers").Log(new[] { 1, 2, 3, 4, 5 }, type: "plot");

            // == Named logging ==
            // Note: this is an experimental feature

            // Example of logging variable with its name
            //  This is good to avoid code like this:
            // 	    var x = GetSomeValue();
            // 		Log("x: " + x);
            //  Then you change the name of 'x' to 'y' and the logging become misleading.
            // Note: To log variable with it's name use the "NamedLog" function, and warp the
            // variable with `new {}` deceleration.
            HL.Log("Example of logging with variable name:");
            var walter = "Also known as Mr.White";
            HL.NamedLog(new {walter});

            // == Synchronous usage ==
            // Note: this is an experimental feature

            // In case you want to use some logging methods from scopes do not allow multi-threading
            // such as invoking method from Visual Studio Immediate Window,
            // use the Synchronous version of the logging methods.

            // Example of synchronous logging
            HL.LogSync("Logging some string, synchronously!");

            // == Watching ==
            // Note: this is an experimental feature
            // Not working right now

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
        }

        /// <summary>
        /// Example of watching a variable in another scope
        /// </summary>
        private static void CheckTheWeather()
        {
            var sky = "Blood moon night";
            HL.Watch(new { sky });
        }
    }
}
