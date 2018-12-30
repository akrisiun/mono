using System;
using System.Diagnostics;

namespace Mono
{
    /// <summary>
    /// Critical argument check class, throw exception if error
    /// </summary>
    public static class Guard
    {
        public static void Check(bool? isTrue, params object[] param)
        {
            if (!isTrue.HasValue || !isTrue.Value)
            {
                throw new ArgumentException(String.Concat(param) ?? "arguments error");
            }
        }

        // Check with Exception assign 
        public static void CheckWithEx(bool? isTrue, Action<Exception> onError = null, params object[] param)
        {
            if (!isTrue.HasValue || isTrue.Value)
                return;
            Exception ex = new ArgumentException(String.Concat(param) ?? "arguments error");
            if (onError != null)
                onError(ex);
            else
                throw ex;
        }


        public static void CheckArgumentNull(object isNull, params object[] param)
        {
            if (isNull == null)
            {
                throw new ArgumentNullException(String.Concat(param) ?? " argument is null error");
            }
        }

        public static void CheckNotNull(object isNull, params object[] param)
        {
            if (isNull == null)
            {
                throw new ArgumentNullException(String.Concat(param) ?? " argument is null error");
            }
        }

        public static void CheckNotNullOrEmpty(object isNull, params object[] param)
        {
            if (isNull == null || (isNull as string).Length == 0)
            {
                throw new ArgumentNullException(String.Concat(param) ?? " argument is null error");
            }
        }
        

        public static void CheckTypeIsAssignableFrom(Type type, Type assignable, params object[] param)
        {
            if (!type.IsAssignableFrom(assignable))
            {
                    throw new ArgumentNullException(
                        String.Concat(param, " argument is not ", assignable));
            }
        }

        public static ILog Log {get; set;}  // not null

        static Guard ()
	    {
            Log = new GuardLog();
	    }

        public class GuardLog : ILog 
        {
            public void Write(string message)
            {
#if DEBUG
                if (LogConsole.ConOut != "2") {
                    System.Diagnostics.Trace.Write(message);
                }
#endif
            }

            public void WriteException(string message, Exception ex)
            {
                LogConsole.Write(message + Environment.NewLine + ex.Message);
            }
        }
    }

    public static class LogConsole
    {
        public static string ConOut {
            get => AppDomain.CurrentDomain.GetData("con.Out") as string ?? "0";
            set {
                AppDomain.CurrentDomain.SetData("con.Out", value ?? "0");
            }
        }

        public static void Write(string msg)
            => Log(msg, "Write");
        public static void WriteException(string message, Exception ex)
            => Write(message + Environment.NewLine + ex.Message);
        public static void Log(string msg, string category = "GET")
        {
            if (ConOut == "1") {
                Console.WriteLine(msg);
            }

            if (ConOut != "2" && Debugger.IsLogging()) {
                Debugger.Log(0, category ?? "GET", msg);
            }
        }
    }

    // Simple log interface
    public interface ILog
    {
        void Write(string message);
        void WriteException(string message, Exception ex);
    }

}
