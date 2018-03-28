using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HypnoLogLib
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

        /// <summary>
        /// Setting used when serializing objects as JSON
        /// </summary>
        private static JsonSerializerSettings JsonSerializerSettings { get; set; }

        #endregion Properties & Data mambers

        static HypnoLog()
        {
            IsInitializes = false;
            IsOn = true;
            IsInFaultedSate = false;
            // TODO: let user set default server by config
            ServerUri = new Uri("http://localhost:7000/");

            // Create some default setting for JSON serialization
            // TODO: let user set JSON serialization settings as well (make property public? via config file?)
            JsonSerializerSettings = new JsonSerializerSettings();
            JsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            JsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }

        #region Events
        /// <summary>
        /// Occurs when the HypnoLog couldn't communicate with the server.
        /// </summary>
        public static event EventHandler ErrorOccurred;
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
        /// <param name="shouldRedirect">If given System.Console output will be redirected to HypnoLog</param>
        /// <param name="serverUri">If given, server URL will be changed.</param>
        [Conditional("DEBUG")]
        public static void Initialize(bool shouldRedirect = false, string serverUri = null)
        {
            // TODO: make Initialize sync (blocking) (?)
            // or make sure no logging will happen before initialization done
            // otherwise Initialize() might be called, not completed, Log() will be called
            // and will start again another initialization.

            // exit if Off
            if (!IsOn) return;

            // Initialize only once
            if (IsInitializes)
            {
                Debug.Print("HypnoLog: Initialization was called more than once");
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
                Debug.Print("HypnoLog: Destination server is down. initialization failed.");
                return;
            }

            // send special 'new session' message
            // TODO: add here await (?)
            SendAsync(ConvertToHypnoLogObject(obj: Guid.NewGuid(), type: "newSession"), checkInitialized: false);

            IsInitializes = true;
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
        /// This is the most simple way to do that.
        /// </summary>
        [Conditional("DEBUG")]
        public static void Log(object obj, string type = null, string[] tags = null)
        {
            SendAsync(ConvertToHypnoLogObject(obj: obj, type: type, tags: tags));
        }

        /// <summary>
        /// Log the given object. Synchronously.
        /// This method doesn't start a new thread, and therefore can be used
        /// from the Immediate Window and Watch Window while debugging in Visual Studio.
        /// Note that using this method will block for each HTTP request to
        /// the server and shouldn't be used normally.
        /// </summary>
        [Conditional("DEBUG")]

        public static void LogSync(object obj, string type = null)
        {
            SendSync(ConvertToHypnoLogObject(obj: obj, type: type));
        }

        /// <summary>
        /// Log the given string.
        /// Replaces the format item in a specified string with the string representation
        /// of a corresponding object in a specified array.
        /// </summary>
        [Conditional("DEBUG")]
        public static void Log(string format, params object[] args)
        {
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
            SendSync(ConvertToHypnoLogObject(String.Format(format, args)));
        }

        /// <summary>
        /// Log variable with its name.
        /// Variable should be warped in new{} statement (Anonymous object) like this:
        /// NamedLog(new { variable })
        /// </summary>
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

        /// <summary>
        /// Sending the given object synchronously to the server.
        /// Doesn't start a new task when sending the data to the server.
        /// Note: HypnoLog must be initialized. Initialization will not happen here.
        /// </summary>
        /// <param name="json">HypnoLog-valid-object to be logged</param>
        private static void SendSync(object json)
        {
            // exit if Off
            if (!IsOn) return;
            // if initialization was not occurred yet, return.
            if (!IsInitializes)
            {
                // TODO: Initialize here as we do in SendAsync. Just make Initialize sync as well.
                Debug.Print("HypnoLog Error: SendSync was called before initialization");
                return;
            }

            // create HTTP Client and send HTTP post request
            var client = new WebClient();
            client.Headers[HttpRequestHeader.ContentType] = "application/json";
            client.Headers[HttpRequestHeader.Accept] = "applicaton/json";

            // convert json object to string.
            var data = JsonConvert.SerializeObject(json, JsonSerializerSettings);

            Uri remote = new Uri(ServerUri.ToString() + "logger/in");
            try
            {
                // TODO: check the  status.
                var status = client.UploadString(remote, data);
            }
            catch
            {
                OnErrorOccurred();
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
        private static async Task<string> SendAsync(object json, bool checkInitialized = true)
        {
            // exit if Off
            if (!IsOn) return null;
            // if initialization was not occurred yet, do it.
            if (checkInitialized && !IsInitializes) Initialize();

            // create HTTP Client and send HTTP post request
            var client = new HttpClient();
            // NOTE: the data sent to server must be valid JSON string
            // NOTE: getting exception here? see project FAQ in README.md
            var result = client.PostAsJsonAsync(ServerUri.ToString() + "logger/in", json).Result;

            string resultContent = result.Content.ReadAsStringAsync().Result;

            try
            {
                var response = result.EnsureSuccessStatusCode();
            }
            catch
            {
                OnErrorOccurred();
                Debug.Print("HypnoLog: Error sending data. result: " + result.StatusCode.ToString());
                // TODO: do something on error (stop logging? log again?..)
                //IsInFaultedSate = true;
            }

            // TODO: return something useful (true/false?) or nothing...
            return resultContent;
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
        private static dynamic ConvertToHypnoLogObject(object obj, string type = null, string name = null, string[] tags = null)
        {
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
                    // TODO: check server status is 200
                    var temp = client.GetStringAsync(ServerUri.ToString() + "logger/status").Result;
                    return true;
                }
                catch (Exception ex)
                {
                    // TODO: log something useful
                    OnErrorOccurred();
                    Debug.Print("HypnoLog: error while checking if destination server is up: " + ex.Message);
                    return false;
                }
            }
        }

        private static Tuple<string, object> ExtractNameAndValue<T>(T data)
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
        private static void RedirectConsoleOutput()
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

        private static void OnErrorOccurred()
        {
            if (ErrorOccurred != null) ErrorOccurred(null, EventArgs.Empty);
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

        #region Extensions for TagsCollection

        // TODO: move all those Extension methods to TagsCollection class, so they can be used directly on TagsCollection object.
        // This is required if we want to allow simple syntax as HL.Tag(..).Log(..)
        // and allow the user not to call "using" for all HypnoLogLib


        //[Conditional("DEBUG")]
        //public static void Log(this TagsCollection tags, object obj, string type = null)
        //{
        //    SendAsync(ConvertToHypnoLogObject(obj: obj, tags: tags.tagsArray, type: null));
        //}

        //[Conditional("DEBUG")]
        //public static void LogSync(this TagsCollection tags, object obj, string type = null)
        //{
        //    SendSync(ConvertToHypnoLogObject(obj: obj, tags: tags.tagsArray, type: type));
        //}

        //[Conditional("DEBUG")]
        //public static void Log(this TagsCollection tags, string format, params object[] args)
        //{
        //    SendAsync(ConvertToHypnoLogObject(String.Format(format, args), tags: tags.tagsArray));
        //}

        //[Conditional("DEBUG")]
        //public static void LogSync(this TagsCollection tags, string format, params object[] args)
        //{
        //    SendSync(ConvertToHypnoLogObject(String.Format(format, args), tags: tags.tagsArray));
        //}

        //[Conditional("DEBUG")]
        //public static void NamedLog<T>(this TagsCollection tags, T obj, string type = null)
        //{
        //    var tuple = ExtractNameAndValue(obj);
        //    SendAsync(ConvertToHypnoLogObject(obj: tuple.Item2, type: type, name: tuple.Item1, tags: tags.tagsArray));
        //}

        //[Conditional("DEBUG")]
        //public static void NamedLogSync<T>(this TagsCollection tags, T obj, string type = null)
        //{
        //    var tuple = ExtractNameAndValue(obj);
        //    SendSync(ConvertToHypnoLogObject(obj: tuple.Item2, type: type, name: tuple.Item1, tags: tags.tagsArray));
        //}

        #endregion Extension for TagsCollection
    }
}
