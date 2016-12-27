using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VDebugLib
{
    public class VDebug
    {

        static private readonly bool isOn = true;
        static private readonly Uri baseAddress = new Uri("http://localhost:7000/vdebug/in");

        static VDebug()
        {
        }

        public static void Init()
        {
            if (!isOn) return;
            Log("New session");
        }

        public static void Log(object obj)
        {
            if (!isOn) return;
            var wait = LogAsync(obj).Result;
        }

        private static async Task<string> LogAsync(object obj)
        {
            // warp any data into valid VDebug JSON
            // This is done because VDebug server expect only JSON content type
            var json = ConvertToVDebugObject(obj);

            // create HTTP Client and send HTTP post request
            var client = new HttpClient();
            // NOTE: the data sent to server must be valid JSON string
            var result = client.PostAsJsonAsync(baseAddress.ToString(), json).Result;

            string resultContent = result.Content.ReadAsStringAsync().Result;

            // TODO: Check that response was successful or throw exception
            //response.EnsureSuccessStatusCode();

            // Read result as Contact
            //Contact result = await response.Content.ReadAsAsync<Contact>();

            return resultContent;
        }

        /// <summary>
        /// Convert given object to valid VDebug object.
        /// VDebug object is object of this type:
        /// { type, value }
        /// Later this object should be converted as is to JSON string.
        /// All simple C# types and numbers array are known types.
        /// Any other C# type will be recognized as "object" and customTyep property
        /// will be add with data about the object type.
        /// </summary>
        private static object ConvertToVDebugObject(object obj)
        {
            // TODO: Handle Enum type better

            var type = obj.GetType();
            if (IsSimple(type))
            {
                return new {type = obj.GetType().Name, value = obj};
            }
            if (IsNumbersArray(type))
            {
                return new {type = "numbersArray", value = obj};
            }
            return new { type = "object",
                customTyep = obj.GetType().Name,
                customFullTyep = obj.GetType().FullName,
                value = obj };
        }

        #region Type related help methods

        /// <summary>
        /// Check if given type is Simple C# type (int, char, double,....)
        /// </summary>
        private static bool IsSimple(Type type)
        {
            return type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(Array);
        }

        /// <summary>
        /// Check if given type is array of numbers
        /// </summary>
        private static bool IsNumbersArray(Type type)
        {
            return type.IsArray && IsNumber(type.GetElementType());
        }

        /// <summary>
        /// Check if given type is a number
        /// </summary>
        private static bool IsNumber(Type type)
        {
            return type == typeof (sbyte)
                || type == typeof (byte)
                || type == typeof (short)
                || type == typeof (ushort)
                || type == typeof (int)
                || type == typeof (uint)
                || type == typeof (long)
                || type == typeof (ulong)
                || type == typeof (float)
                || type == typeof (double)
                || type == typeof (decimal);
        }

        #endregion Type related help methods
    }
}
