using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HypnoLog
{
    /// <summary>
    /// HypnoLog is a logging service class.
    /// This class should work only under Debug, that's why all public methods
    /// under this class are marked with Conditional("DEBUG") attribute.
    /// </summary>
    public static class HypnoLog
    {
        #region Properties & Data members

        /// <summary>
        /// True if initialization process happed successful
        /// </summary>
        public static bool IsInitialized { get; private set; }

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

        /// <summary>
        /// Counter for how many errors occurred
        /// </summary>
        public static int ErrorsCounter { get; private set; }

        private static Uri ServerUri { get; set; }

        /// <summary>
        /// Setting used when serializing objects as JSON
        /// </summary>
        private static JsonSerializerSettings JsonSerializerSettings { get; set; }

        #endregion Properties & Data mambers

        /// <summary>
        /// Static constructor.
        /// Called by .NET framework when class is first accessed.
        /// </summary>
        static HypnoLog()
        {
            IsInitialized = false;
            ErrorsCounter = 0;
            IsOn = true;
            IsInFaultedSate = false;

            // Create some default setting for JSON serialization
            // TODO: let user set JSON serialization settings as well (make property public? via config file?)
            JsonSerializerSettings = new JsonSerializerSettings();
            JsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            JsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }

        #region Events
        /// <summary>
        /// Occurs when the HypnoLog encounter in some error.
        /// (such as, couldn't communicate with the server)
        /// </summary>
        public static event Action<object, Exception> ErrorOccurred;

        #endregion

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
        /// <param name="host">HypnoLog server host name. Default is `localhost`</param>
        /// <param name="port">HypnoLog server port number. Default is `7000`</param>
        /// <param name="shouldRedirect">If given `System.Console` output will be redirected to HypnoLog. Default is false.</param>
        [Conditional("DEBUG")]
        public static void Initialize(string host = "localhost", ushort port = 7000, bool shouldRedirect = false)
        {
            // TODO: make Initialization async, but blocking other logging operations.
            // this means, initialization will not block the code outer flow,
            // but if some logging operation was called, before initialization complete, it will wait.
            // Also, provide a sync Initialize() version, for sync logging.

            // TODO: add error handling callback into init method

            // exit if Off
            if (!IsOn) return;

            // Initialize only once
            if (IsInitialized)
            {
                Debug.Print("HypnoLog: Initialization was called more than once");
                return;
            }

            // Set server URL if given
            ServerUri = new Uri(String.Format("http://{0}:{1}/", host, port));

            // Do not continue if server is down
            if (!IsServerUp())
            {
                Disable();
                IsInFaultedSate = true;
                Debug.Print("HypnoLog: Destination server is down. initialization failed.");
                return;
            }

            // send special 'new session' message
            // TODO: add here await (?)
            SendAsync(ConvertToHypnoLogObject(obj: Guid.NewGuid(), type: "newSession"), checkInitialized: false);

            IsInitialized = true;
            if(shouldRedirect) RedirectConsoleOutput();
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
        /// </summary>
        /// <param name="obj">Object to be logged</param>
        /// <param name="type">String represent the type of the logged object, will determine how the object is visualized. If not provided, `object` type will be used</param>
        [Conditional("DEBUG")]
        public static void Log(object obj, string type = null)
        {
            SendAsync(ConvertToHypnoLogObject(obj: obj, type: type));
        }

        // TODO: consider, instead of duplicating all the Log function with a "sync" version,
        // providing some ..Log(..).Sync(); syntax.
        // For this all async log function should return a value, yes, so conditional debug
        // connot be used. maybe check for debug mode only once while initializing,
        // and then we can also provide by config whether we want to use HL only in debug
        // mode or in any mode.

        /// <summary>
        /// Log the given object. Synchronously.
        /// This method doesn't start a new thread, and therefore can be used
        /// from the Immediate Window and Watch Window while debugging in Visual Studio.
        /// Note that using this method will block for each HTTP request to
        /// the server and shouldn't be used normally.
        /// </summary>
        /// <param name="obj">Object to be logged</param>
        /// <param name="type">String represent the type of the logged object, will determine how the object is visualized. If not provided, `object` type will be used</param>
        [Conditional("DEBUG")]

        public static void LogSync(object obj, string type = null)
        {
            SendSync(ConvertToHypnoLogObject(obj: obj, type: type));
        }

        /// <summary>
        /// Log the given string.
        /// Replaces the format item in a specified string with the string representation
        /// of a corresponding object in a specified array.
        /// Logging type will be `string`.
        /// </summary>
        [Conditional("DEBUG")]
        public static void Log(string format, params object[] args)
        {
            // If no string given, log as `null` object
            if (format == null)
            {
                Log(obj: "null", type: "object");
                return;
            }

            SendAsync(ConvertToHypnoLogObject(String.Format(format, args)));
        }

        /// <summary>
        /// Log the given string. Synchronously.
        /// Replaces the format item in a specified string with the string representation
        /// of a corresponding object in a specified array.
        /// This method doesn't start a new thread, and therefore can be used
        /// from the Immediate Window and Watch Window while debugging in Visual Studio.
        /// Note that using this method will block for each HTTP request to
        /// the server and shouldn't be used normally.
        /// </summary>
        [Conditional("DEBUG")]
        public static void LogSync(string format, params object[] args)
        {
            // If no string given, log as `null` object
            if (format == null)
            {
                Log(obj: "null", type: "object");
                return;
            }

            SendSync(ConvertToHypnoLogObject(String.Format(format, args)));
        }

        /// <summary>
        /// Log variable with its name.
        /// Variable should be warped in new{} statement (Anonymous object) like this:
        /// NamedLog(new { variable })
        /// </summary>
        /// <param name="obj">Object to be logged</param>
        /// <param name="type">String represent the type of the logged object, will determine how the object is visualized. If not provided, `object` type will be used</param>
        [Conditional("DEBUG")]
        public static void NamedLog<T>(T obj, string type = null)
        {
            // TODO: handle somehow wrong usage of "NamedLog" function (?). (when not warping variable with new{})

            var tuple = ExtractNameAndValue(obj);
            SendAsync(ConvertToHypnoLogObject(obj: tuple.Item2, type: type, name: tuple.Item1));
        }

        /// <summary>
        /// Log variable with its name.
        /// Variable should be warped in new{} statement (Anonymous object) like this:
        /// NamedLog(new { variable })
        /// </summary>
        /// <param name="obj">Object to be logged</param>
        /// <param name="type">String represent the type of the logged object, will determine how the object is visualized. If not provided, `object` type will be used</param>
        [Conditional("DEBUG")]
        public static void NamedLogSync<T>(T obj, string type = null)
        {
            // TODO: handle somehow wrong usage of "NamedLog" function (?). (when not warping variable with new{})

            var tuple = ExtractNameAndValue(obj);
            SendSync(ConvertToHypnoLogObject(obj: tuple.Item2, type: type, name: tuple.Item1));
        }

        /// <summary>
        /// Watch variable with its name.
        /// Variable should be warped in new{} statement (Anonymous object) like this:
        /// WatchLog(new { variable })
        /// </summary>
        [Conditional("DEBUG")]
        public static void Watch<T>(T obj, string type = null)
        {
            var callingMethod = new StackFrame(1, true).GetMethod();
            var scope = callingMethod.ReflectedType.FullName + "." + callingMethod.Name;
            var firstProperty = typeof(T).GetProperties()[0];

            dynamic json = ConvertToHypnoLogObject(obj: firstProperty.GetValue(obj), type: type, name: firstProperty.Name);

            json.debugOption = "watch";
            json.fullName = scope + "." + firstProperty.Name;

            SendAsync(json);
        }

        /// <summary>
        /// Return a TagsCollection object, this allow's to add tags to the data being logged.
        /// After invoking this method, the user can call any of the log method.
        /// TODO: Add conditional DEBUG. (check what to do if function return value)
        /// </summary>
        public static TagsCollection Tag(string tags)
        {
            return new TagsCollection(tags);
        }

        #endregion Public methods

        #region Private methods

        // TODO: combine SendSync and SendAsync to be with more shared code.
        // make the sending methods agnostic to the logged object,
        // just receive or (a) already serialized JSON string which can be sent
        // or (b) C# object which can be serialized to JSON string and then sent
        // probably (a) will be more flexible (leaving the serialization logic out)

        /// <summary>
        /// Sending the given object synchronously to the server.
        /// Note: This method initialize HypnoLog if not initialized.
        /// </summary>
        /// <param name="json">HypnoLog-valid-object to be logged</param>
        /// <param name="checkInitialized">Should initialization be checked before sending the data</param>
        internal static void SendSync(object json, bool checkInitialized = true)
        {
            // check conditions before sending (such as initialized, set to `On`,..)
            if (!CheckSendCondition(checkInitialized)) return;

            try
            {
                // create HTTP Client and send HTTP post request
                var client = new WebClient();
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers[HttpRequestHeader.Accept] = "applicaton/json";

                // convert json object to string.
                var data = JsonConvert.SerializeObject(json, JsonSerializerSettings);

                Uri remote = new Uri(baseUri: ServerUri, relativeUri: "logger/in");
                // TODO: check the  status.
                var status = client.UploadString(remote, data);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(json, ex);
                Debug.Print("HypnoLog: Error sending data");
                // TODO: do something on error (stop logging? log again?..)
                //IsInFaultedSate = true;
            }
        }

        /// <summary>
        /// Sending the given object asynchronously to the server.
        /// Note: This method initialize HypnoLog if not initialized.
        /// <param name="json">HypnoLog-valid-object to be logged</param>
        /// <param name="checkInitialized">Should initialization be checked before sending the data</param>
        /// </summary>
        internal static async Task<bool> SendAsync(object json, bool checkInitialized = true)
        {
            // check conditions before sending (such as initialized, set to `On`,..)
            if (!CheckSendCondition(checkInitialized)) return false;

            HttpResponseMessage result = null;
            try
            {
                // create HTTP Client and send HTTP post request
                var client = new HttpClient();
                // NOTE: the data sent to server must be valid JSON string
                // NOTE: getting exception here? see project FAQ in README.md
                result = client.PostAsJsonAsync(new Uri(baseUri: ServerUri, relativeUri: "logger/in"), json).Result;

                //string resultContent = result.Content.ReadAsStringAsync().Result;
                var response = result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(json, ex);
                Debug.Print("HypnoLog: Error sending data");
                // TODO: do something on error (stop logging? log again?..)
                //IsInFaultedSate = true;
                return false;
            }

            return true;
        }

        private static bool CheckSendCondition(bool checkInitialized = true)
        {
            // exit if Off
            if (!IsOn) return false;
            // if initialization was not occurred yet, do it.
            if (checkInitialized && !IsInitialized)
            {
                Initialize();
                // check if initialization was successful
                if (!IsOn || !IsInitialized || IsInFaultedSate) return false;
            }
            return true;
        }

        /// <summary>
        /// Convert given object to valid HypnoLog object.
        /// HypnoLog object is object of this type:
        /// { type, data }
        /// Later this object should be converted as is to JSON string.
        /// All simple C# types and numbers array are known types.
        /// Any other C# type will be recognized as "object" and customType property
        /// will be add with data about the object type.
        /// </summary>
        /// <param name="obj">Object to be converted</param>
        /// <param name="type">Optional. Type of the given object. Determine how to object will be visualized.</param>
        /// <param name="name">Optional. Name of the variable being logged. Should be provided from user only by NamedLog method.</param>
        /// <param name="tags">Optional. Tags to include in the Log Object</param>
        internal static dynamic ConvertToHypnoLogObject(object obj, string type = null, string name = null, string[] tags = null)
        {
            if (obj == null)
            {
                obj = "null";
                type = "object";
            }

            // TODO: use some sort of HypnoLog-log object that extend ExpandoObject but require type and data properties.
            dynamic json = new ExpandoObject();
            json.data = obj;
            json.type = type ?? DetermineObjectType(obj);

            // set optional fields if given

            if (name != null)
                json.name = name;

            if (tags != null)
                json.tags = tags;

            return json;
        }


        /// <summary>
        /// Try to determine given object type
        /// </summary>
        /// <returns></returns>
        private static string DetermineObjectType(object obj)
        {
            // first get C# Type
            var type = obj.GetType();

            // If simple (primitive) C# type, just use it by it's name
            if (IsSimple(type))
            {
                return obj.GetType().Name;
            }
            // else, try some custom more complex types we know
            // try number array
            else if (IsNumbersArray(type))
            {
                return "numbersArray";
            }
            // else, if all unknown, set it as "object"
            else
            {
                return "object";
            }

            // TODO: Handle Enum type better

            // TODO: use C# type name?
            //json.customType = type.Name;
            //json.customFullType = type.FullName;
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
                    var respons = client.GetStringAsync(new Uri(baseUri: ServerUri, relativeUri: "logger/status")).Result;
                    if (respons != "200")
                        throw new Exception("Server response code is not 200. result: " + respons);
                    return true;
                }
                catch (Exception ex)
                {
                    // TODO: log something useful
                    OnErrorOccurred(null, ex);
                    Debug.Print("HypnoLog: error while checking if destination server is up: " + ex.Message);
                    return false;
                }
            }
        }

        internal static Tuple<string, object> ExtractNameAndValue<T>(T data)
        {
            var firstProperty = typeof(T).GetProperties()[0];
            var name = firstProperty.Name;
            var value = firstProperty.GetValue(data);

            return Tuple.Create<string, object>(name, value);
        }

        // NOTE: Redirect-console-output is argument in initialization and not method by itself because
        // when asked to redirect console output we need to know if we have working HL server (= initialization was successful),
        // we do not want to redirect console output and then realize initialization failed.
        // This is also because initialization is non-blocking.

        /// <summary>
        /// After calling this method all System.Console output will be redirected to HypnoLog.
        /// This means any output such as 'Console.WriteLine("text")' will be
        /// redirected to HypnoLog instead of System Console.
        /// This is done by setting the System.Console.Out and System.Control.Error
        /// property to internal HypnoLog text writer.
        /// </summary>
        public static void RedirectConsoleOutput()
        {
            Console.WriteLine("Console output redirected to HypnoLog.");
            Debug.Print("Console output redirected to HypnoLog.");

            // Create text writer which Log all input directly using HypnoLog
            // and set it as Console Out and Error writers
            var writer = new LogTextWriter();
            Console.SetOut(writer);
            // TODO: Mark somehow errors in HypnoLog
            Console.SetError(writer);
        }

        private static void OnErrorOccurred(object obj = null, Exception ex = null)
        {
            ErrorsCounter++;
            // TODO: Do something we have repeated logging errors, like disable HypnoLog (?)

            // If user provide error handling method, call it
            if (ErrorOccurred != null)
            {
                ErrorOccurred(obj, ex);
            }
            // otherwise, use some default error handling logic
            else
            {
                Console.WriteLine(  "HypnoLog error occurred: {0}", (ex != null ? ex.ToString() : String.Empty));
                Debug.Print(        "HypnoLog error occurred: {0}", (ex != null ? ex.ToString() : String.Empty));
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
    }
}
