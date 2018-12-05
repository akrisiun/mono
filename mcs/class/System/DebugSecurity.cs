using System.ComponentModel;

namespace System.Diagnostics.Security
{
    public class Debug
    {
        // TraceEventType.Information

        public static void Assert(bool ok, string format = null, params object[] args)
        {
            if (ok) {
                Console.WriteLine(string.Format(format, args));
            }
        }

        public static void Fail(string format, params object[] args)
            => Console.WriteLine(string.Format(format, args));

        public static void WriteLine(string format, params object[] args)
            => Console.WriteLine(string.Format(format, args));
    }

    public enum TraceEventType {
        Critical    = 0x01,
        Error       = 0x02,
        Warning     = 0x04,
        Information = 0x08,
        Verbose     = 0x10,

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Start       = 0x0100,
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Stop        = 0x0200,
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Suspend     = 0x0400,
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Resume      = 0x0800,
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Transfer    = 0x1000,
    }
}
