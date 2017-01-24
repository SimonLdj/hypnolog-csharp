using System;
using System.Diagnostics;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;

namespace VDebugLib
{
    /// <summary>
    /// VDebug is a logging service class.
    /// This class should work only under Debug, that's why all public methods
    /// under this class are marked with Conditional("DEBUG") attribute.
    /// </summary>
    public static class VDebug
    {
        #region Properties & Data members

        /// <summary>
        /// True if initialization process happed successful
        /// </summary>
        public static bool IsInitializes { get; private set; }

        // TODO: find better way to keep logger on/off ? with a global flag?
        /// <summary>
        /// Whether the logger is On or Off. true if on.
        /// By default this is On.
        /// </summary>
        public static bool IsOn { get; private set; }

        /// <summary>
        /// True if logger in faulted state.
        /// This may occur as a result of failure connecting to the server.
        /// </summary>
        public static bool IsInFaultedSate { get; private set; }

        private static Uri ServerUri { get; set; }

        #endregion Properties & Data mambers

        static VDebug()
        {
            IsInitializes = false;
            IsOn = true;
            IsInFaultedSate = false;
            // TODO: let user set default server by config
            ServerUri = new Uri("http://localhost:7000/");
        }

        #region Public methods

        // NOTE: All public methods should be marked with [Conditional("DEBUG")] attribute

        /// <summary>
        /// Initialize the logger. Mark begging of new session.
        /// It is not mandatory to call Initialize but it is recommended to do it as soon as
        /// possible in the begging of the program. This will help marking the begging
        /// of new session in a proper way.
        /// If Initialize was not invoked manually, it will be called implicitly at the first
        /// logging action.
        /// </summary>
        /// <param name="serverUri">If given, server URL will be changed.</param>
        [Conditional("DEBUG")]
        public static void Initialize(string serverUri = null)
        {
            // exit if Off
            if (!IsOn) return;

            // Initialize only once
            if (IsInitializes)
            {
                Debug.Print("VDebug: Initialization was called more than once");
                return;
            }

            // Set new server URL if given
            if (serverUri != null)
            {
                ServerUri = new Uri(serverUri);
            }

            // Do not continue if server is down
            if (!IsServerUp())
            {
                Disable();
                IsInFaultedSate = true;
                Debug.Print("VDebug: Destination server is down. initialization failed.");
                return;
            }

            // send special 'new session' message
            // TODO: make new session type more strongly typed (?)
            var newSessionObj = new { type = "newSession", value = Guid.NewGuid() };
            var wait = SendAsync(newSessionObj).Result;

            IsInitializes = true;
        }

        /// <summary>
        /// Disable the logger.
        /// </summary>
        [Conditional("DEBUG")]
        public static void Disable()
        {
            IsOn = false;
        }

        /// <summary>
        /// Try to Enable the logger.
        /// If logger in faulted state this may not succeed.
        /// </summary>
        [Conditional("DEBUG")]
        public static void Enable()
        {
            if (IsInFaultedSate) return;
            IsOn = true;
        }

        /// <summary>
        /// Log the given object.
        /// This is the most simple way to do that.
        /// </summary>
        [Conditional("DEBUG")]
        public static void Log(object data)
        {
            Send(ConvertToVDebugObject(data));
        }

        /// <summary>
        /// Log variable with its name.
        /// Variable should be warped in new{} statement (Anonymous object) like this:
        /// NamedLog(new { variable })
        /// </summary>
        [Conditional("DEBUG")]
        public static void NamedLog<T>(T data)
        {
            // TODO: handle somehow wrong usage of "NamedLog" function (?). (when not warping variable with new{})

            var firstProperty = typeof (T).GetProperties()[0];
            var name = firstProperty.Name;
            var value = firstProperty.GetValue(data);
            Send(ConvertToVDebugObject(value, name));
        }

        /// <summary>
        /// Wach variable with this name from that sorce.
        /// Variable should be warped in new{} statement (Anonymous object) like this:
        /// WatchLog(new { variable })
        /// </summary>
        [Conditional("DEBUG")]
        public static void Watch<T>(T data)
        {
            var callingMethod = new StackFrame(1, true).GetMethod();
            var scope = callingMethod.ReflectedType.FullName + "." + callingMethod.Name;
            var firstProperty = typeof(T).GetProperties()[0];

            dynamic json = ConvertToVDebugObject(firstProperty.GetValue(data), firstProperty.Name);

            json.debugOption = "watch";
            json.fullName = scope + "." + firstProperty.Name;

            Send(json);

        }

        /// <summary>
        /// Return a hypnoTag object, this allow's to add tags to the data being loged.
        /// After invoking this method, the user can call any of the log method.
        /// TODO: Add conditional DEBUG. (check what to do if function return value)
        /// </summary>
        public static TagsCollection Tag(string tags)
        {
            return new TagsCollection(tags);
        }

        #endregion Public methods

        #region Private methods

        /// <summary>
        /// The main sync method to Log.
        /// This method is private to avoid miss using it and to avoid complicated public API.
        /// </summary>
        /// <param name="json">Vdbug-valid-object to be logged</param>
        /// <param name="name">Optional. Name of the variable being logged. Should be provided from user only by NamedLog method.</param>
        private static void Send(object json)
        {
            // exit if Off
            if (!IsOn) return;

            // if initialization was not occurred yet, do it.
            if (!IsInitializes)
                Initialize();

            var wait = SendAsync(json).Result;
        }

        private static async Task<string> SendAsync(object json)
        {
            // create HTTP Client and send HTTP post request
            var client = new HttpClient();
            // NOTE: the data sent to server must be valid JSON string
            // NOTE: getting exception here? see project FAQ in README.md
            var result = client.PostAsJsonAsync(ServerUri.ToString() + "vdebug/in", json).Result;

            string resultContent = result.Content.ReadAsStringAsync().Result;

            try
            {
                var response = result.EnsureSuccessStatusCode();
            }
            catch
            {
                Debug.Print("VDebug: Error sending data. result: " + result.StatusCode.ToString());
                // TODO: do something on error (stop logging? log again?..)
                //IsInFaultedSate = true;
            }

            // TODO: return something useful (true/false?) or nothing...
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
        private static dynamic ConvertToVDebugObject(object obj, string name = null, string[] tags = null)
        {
            // TODO: Handle Enum type better
            var type = obj.GetType();
            // TODO: use some sort of VDebug-log object that extend ExpandoObject but require type and value properties.
            dynamic json = new ExpandoObject();
            json.value = obj;
            json.type = "unknown";

            if (name != null)
                json.name = name;

            if (tags != null)
                json.tags = tags;

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

        /// <summary>
        /// Check if destination server is up and running.
        /// </summary>
        /// <returns>True if up</returns>
        private static bool IsServerUp()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // TODO: check server status is 200
                    var temp = client.GetStringAsync(ServerUri.ToString() + "vdebug/status").Result;
                    return true;
                }
                catch (Exception ex)
                {
                    // TODO: log something useful
                    Debug.Print("VDebug: error while checking if destination server is up: " + ex.Message);
                    return false;
                }
            }
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

        #region Extensions

        [Conditional("DEBUG")]
        public static void Log(this TagsCollection tags, object data)
        {
            Send(ConvertToVDebugObject(data, name: null, tags: tags.tagsArray));
        }

        [Conditional("DEBUG")]
        public static void NamedLog<T>(this TagsCollection tags, T data)
        {
            var firstProperty = typeof(T).GetProperties()[0];
            var name = firstProperty.Name;
            var value = firstProperty.GetValue(data);
            Send(ConvertToVDebugObject(value, name, tags: tags.tagsArray));
        }
        #endregion Extension
    }
}
