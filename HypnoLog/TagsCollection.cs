using System;
using System.Diagnostics;

namespace HypnoLog
{
    public class TagsCollection
    {
        public string[] tagsArray;
        private static char[] separator = { '#', ' ' };
        public TagsCollection(string tags)
        {
            tagsArray = tags.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        }

        #region logging methods
        // Logging methods which can be chained to the tags object to receive syntax like:
        // HL.Tag("someTag").Log("Some data here");

        [Conditional("DEBUG")]
        public void Log(object obj, string type = null)
        {
            HypnoLog.SendAsync(HypnoLog.ConvertToHypnoLogObject(obj: obj, tags: this.tagsArray, type: null));
        }

        [Conditional("DEBUG")]
        public void LogSync(object obj, string type = null)
        {
            HypnoLog.SendSync(HypnoLog.ConvertToHypnoLogObject(obj: obj, tags: this.tagsArray, type: type));
        }

        [Conditional("DEBUG")]
        public void Log(string format, params object[] args)
        {
            HypnoLog.SendAsync(HypnoLog.ConvertToHypnoLogObject(String.Format(format, args), tags: this.tagsArray));
        }

        [Conditional("DEBUG")]
        public void LogSync(string format, params object[] args)
        {
            HypnoLog.SendSync(HypnoLog.ConvertToHypnoLogObject(String.Format(format, args), tags: this.tagsArray));
        }

        [Conditional("DEBUG")]
        public void NamedLog<T>(T obj, string type = null)
        {
            var tuple = HypnoLog.ExtractNameAndValue(obj);
            HypnoLog.SendAsync(HypnoLog.ConvertToHypnoLogObject(obj: tuple.Item2, type: type, name: tuple.Item1, tags: this.tagsArray));
        }

        [Conditional("DEBUG")]
        public void NamedLogSync<T>(T obj, string type = null)
        {
            var tuple = HypnoLog.ExtractNameAndValue(obj);
            HypnoLog.SendSync(HypnoLog.ConvertToHypnoLogObject(obj: tuple.Item2, type: type, name: tuple.Item1, tags: this.tagsArray));
        }

        #endregion logging methods
    }
}
