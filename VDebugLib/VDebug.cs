using System;
using System.Dynamic;
using System.Net.Http;
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

        #region Public methods

        public static void Init()
        {
            if (!isOn) return;

            // create special 'new session' type
            // TODO: make new session type more strongly typed (?)
            var newSessionObj = new {type = "newSession", value = Guid.NewGuid()};
            var wait = LogAsync(newSessionObj).Result;
        }

        public static void Log(object data, string name = null)
        {
            if (!isOn) return;

            // warp any data into valid VDebug JSON
            // This is done because VDebug server expect only JSON content type
            dynamic json = ConvertToVDebugObject(data);

            if (name != null)
                json.name = name;

            var wait = LogAsync(json).Result;
        }

        /// <summary>
        /// Log variable with its name.
        /// Variable should be warped in new{} statement (Anonymous object) like this:
        /// NameLog(new { variable })
        /// </summary>
        public static void NameLog<T>(T data)
        {
            // TODO: handle somehow wrong usage of "NameLog" function (?). (when not warping variable with new{})

            var firstProperty = typeof (T).GetProperties()[0];
            var name = firstProperty.Name;
            var value = firstProperty.GetValue(data);
            Log(value, name);
        }

        #endregion Public methods

        #region Private methods

        private static async Task<string> LogAsync(object json)
        {
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
        /// Any other C# type will be recognized as "object" and customType property
        /// will be add with data about the object type.
        /// </summary>
        private static dynamic ConvertToVDebugObject(object obj)
        {
            // TODO: Handle Enum type better

            var type = obj.GetType();

            // TODO: use some sort of VDebug-log object that extend ExpandoObject but require type and value properties.
            dynamic json = new ExpandoObject();
            json.value = obj;
            json.type = "unknown";

            if (IsSimple(type))
            {
                json.type = obj.GetType().Name;
            }
            else if (IsNumbersArray(type))
            {
                json.type = "numbersArray";
            }
            else
            {
                json.type = "object";
                json.customType = type.Name;
                json.customFullType = type.FullName;
            }

            return json;
        }

        #endregion Private methods

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
