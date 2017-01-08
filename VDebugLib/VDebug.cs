using System;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;

namespace VDebugLib
{
    public class VDebug
    {
        #region Properties & Data members

        /// <summary>
        /// True if initialization process happed successful
        /// </summary>
        public static bool IsInitializes { get; private set; }
        static private readonly bool isOn = true;
        static private readonly Uri baseAddress = new Uri("http://localhost:7000/vdebug/in");

        #endregion Properties & Data mambers
        static VDebug()
        {
            IsInitializes = false;
        }

        #region Public methods

        /// <summary>
        /// Initialize the logger. Mark begging of new session.
        /// It is not mandatory to call Initialize but it is recommended to do it as soon as
        /// possible in the begging of the program. This will help marking the begging
        /// of new session in a proper way.
        /// If Initialize was not invoked manually, it will be called implicitly at the first
        /// logging action.
        /// </summary>
        public static void Initialize()
        {
            if (!isOn) return;

            // Initialize only once
            if (IsInitializes) return;

            // TODO: do not continue if server is down
            //isOn = IsURLValid();

            // send special 'new session' message
            // TODO: make new session type more strongly typed (?)
            var newSessionObj = new { type = "newSession", value = Guid.NewGuid() };
            var wait = LogAsync(newSessionObj).Result;

            IsInitializes = true;
        }

        /// <summary>
        /// Log the given object.
        /// This is the most simple way to do that.
        /// </summary>
        public static void Log(object data)
        {
            Log(data, null);
        }

        /// <summary>
        /// Log variable with its name.
        /// Variable should be warped in new{} statement (Anonymous object) like this:
        /// NamedLog(new { variable })
        /// </summary>
        public static void NamedLog<T>(T data)
        {
            // TODO: handle somehow wrong usage of "NamedLog" function (?). (when not warping variable with new{})

            var firstProperty = typeof (T).GetProperties()[0];
            var name = firstProperty.Name;
            var value = firstProperty.GetValue(data);
            Log(value, name);
        }

        #endregion Public methods

        #region Private methods

        /// <summary>
        /// The main sync method to Log.
        /// This method is private to avoid miss using it and to avoid complicated public API.
        /// </summary>
        /// <param name="data">The object to be logged</param>
        /// <param name="name">Optional. Name of the variable being logged. Should be provided from user only by NamedLog method.</param>
        private static void Log(object data, string name = null)
        {
            if (!isOn) return;

            // if initialization was not occurred yet, do it.
            if (!IsInitializes)
                Initialize();

            // warp any data into valid VDebug JSON
            // This is done because VDebug server expect only JSON content type
            dynamic json = ConvertToVDebugObject(data);

            if (name != null)
                json.name = name;

            var wait = LogAsync(json).Result;
        }

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
