using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HypnoLogLib
{
    /// <summary>
    /// Override all TextWriter methods to log directly using HypnoLog.
    /// This class is used to replace the TextWriter in System.Console
    /// to redirect System.Console output to HypnoLog.
    /// </summary>
    internal class LogTextWriter : TextWriter
    {
        // TODO: Complete implementation of all overridden methods

        public override Encoding Encoding { get { return Encoding.UTF8; } }

        public override void Write(bool value) { HypnoLog.Log(value); }
        public override void Write(char value) { HypnoLog.Log(value); }
        public override void Write(char[] value) { HypnoLog.Log(value); }
        public override void Write(decimal value) { HypnoLog.Log(value); }
        public override void Write(double value) { HypnoLog.Log(value); }
        public override void Write(float value) { HypnoLog.Log(value); }
        public override void Write(int value) { HypnoLog.Log(value); }
        public override void Write(long value) { HypnoLog.Log(value); }
        public override void Write(object value) { HypnoLog.Log(value); }
        public override void Write(string value) { HypnoLog.Log(value); }
        [CLSCompliant(false)]
        public override void Write(uint value) { HypnoLog.Log(value); }
        [CLSCompliant(false)]
        public override void Write(ulong value) { HypnoLog.Log(value); }
        public override void Write(string format, object arg0) { HypnoLog.Log(System.String.Format(format, arg0)); }
        public override void Write(string format, params object[] arg) { HypnoLog.Log(System.String.Format(format, arg)); }
        public override void Write(char[] buffer, int index, int count) { throw new NotImplementedException(); }
        public override void Write(string format, object arg0, object arg1) { HypnoLog.Log(System.String.Format(format, arg0, arg1)); }
        public override void Write(string format, object arg0, object arg1, object arg2) { HypnoLog.Log(System.String.Format(format, arg0, arg1, arg2)); }
        public override Task WriteAsync(char value) { throw new NotImplementedException(); }
        public new Task WriteAsync(char[] value) { throw new NotImplementedException(); }
        public override Task WriteAsync(string value) { throw new NotImplementedException(); }
        public override Task WriteAsync(char[] buffer, int index, int count) { throw new NotImplementedException(); }
        public override void WriteLine() { HypnoLog.Log(""); }
        public override void WriteLine(bool value) { HypnoLog.Log(value); }
        public override void WriteLine(char value) { HypnoLog.Log(value); }
        public override void WriteLine(char[] buffer) { HypnoLog.Log(buffer); }
        public override void WriteLine(decimal value) { HypnoLog.Log(value); }
        public override void WriteLine(double value) { HypnoLog.Log(value); }
        public override void WriteLine(float value) { HypnoLog.Log(value); }
        public override void WriteLine(int value) { HypnoLog.Log(value); }
        public override void WriteLine(long value) { HypnoLog.Log(value); }
        public override void WriteLine(object value) { HypnoLog.Log(value); }
        public override void WriteLine(string value) { HypnoLog.Log(value); }
        public override void WriteLine(uint value) { HypnoLog.Log(value); }
        public override void WriteLine(ulong value) { HypnoLog.Log(value); }
        public override void WriteLine(string format, object arg0) { HypnoLog.Log(System.String.Format(format, arg0)); }
        public override void WriteLine(string format, params object[] arg) { HypnoLog.Log(System.String.Format(format)); }
        public override void WriteLine(char[] buffer, int index, int count) { throw new NotImplementedException(); }
        public override void WriteLine(string format, object arg0, object arg1) { HypnoLog.Log(System.String.Format(format, arg0, arg1)); }
        public override void WriteLine(string format, object arg0, object arg1, object arg2) { HypnoLog.Log(System.String.Format(format, arg0, arg1, arg2)); }
        public override Task WriteLineAsync() { throw new NotImplementedException(); }
        public override Task WriteLineAsync(char value) { throw new NotImplementedException(); }
        public new Task WriteLineAsync(char[] buffer) { throw new NotImplementedException(); }
        public override Task WriteLineAsync(string value) { throw new NotImplementedException(); }
        public override Task WriteLineAsync(char[] buffer, int index, int count) { throw new NotImplementedException(); }
    }
}
