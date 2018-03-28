using System;
using System.Linq;

// Do this to easily use HypnoLog as HL in your code
using HL = HypnoLogLib.HypnoLog;

namespace HypnoLogExample
{
    /// <summary>
    /// Basic HypnoLog usage example
    /// </summary>
    public static class BasicExample
    {
        public static void Run()
        {
            // Log a string
            string str = "Hello HypnoLog from C#!";
            HL.Log(str);

            // Log array of numbers as single line graph ("plot")
            Random randNum = new Random();
            int[] exampleArray = Enumerable.Range(0, 50).Select(i => (i * 10) % 100 + (int)Math.Floor(randNum.NextDouble() * 10)).ToArray();
            HL.Log("Example of logging array of numbers as single line graph (plot):");
            HL.Log(exampleArray, "plot");

            // TODO: log multi line graph

            // Log 2d array as Heatmap
            int[,] arr2d = new int[10,10];
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    //arr2d[i,j] = randNum.Next(1, 50);
                    arr2d[i, j] = i*10 + j;
                }
            }
            HL.Log("Example of logging 2d number array as Heatmap:");
            HL.Log(arr2d, "heatmap");

            // TODO: log histogram

            // log Lat-Long Geo locations using Google maps
            var locations = new object[]
            {
                new object[] {"Lat", "Long", "Name"           },
                new object[] {37.4232, -122.0853, "Work"      },
                new object[] {37.4289, -122.1697, "University"},
                new object[] {37.6153, -122.3900, "Airport"   },
                new object[] {37.4422, -122.1731, "Shopping"  }
            };
            HL.Log("Example of logging Lat-Long Geo locations using Google maps:");
            HL.Log(locations, "GoogleMaps");

            // Log a custom object of defined Class type
            var board = new Surfboard()
            {
                Name = "Lib Tech Bowl",
                Length = 6.2,
                Volume = 30.8
            };
            HL.Log("Example of logging a custom object of defined Class type");
            HL.Log(board);

            // Log a custom object of anonymous type, with nested object
            var car = new
                {
                    BrandName = "Seat",
                    ModelName = "Mii",
                    Engine = new
                    {
                        NumberOfCylinders = 3,
                        Acceleration = 14.4
                    },
                    Color = "Red"
                };
            HL.Log("Example of logging an custom object of anonymous type");
            HL.Log(car);
        }

        private class Surfboard
        {
            public string Name { get; set; }
            public double Length { get; set; }
            public double Volume { get; set; }
        }
    }
}
