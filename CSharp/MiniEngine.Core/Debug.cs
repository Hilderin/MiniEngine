
using System;

namespace MiniEngine
{
    /// <summary>
    /// Debug class
    /// </summary>
    public static class Debug
    {
        internal static DebugTraceListener DebugTraceListener;

        /// <summary>
        /// Print a debug info message
        /// </summary>
        public static void Info(string message)
        {
            if (DebugTraceListener != null)
                DebugTraceListener.WriteLine(message);
            else
                System.Diagnostics.Debug.WriteLine(message);
        }

        /// <summary>
        /// Print a debug error message
        /// </summary>
        public static void Error(string message)
        {
            if (DebugTraceListener != null)
                DebugTraceListener.WriteLine("ERROR: " + message);
            else
                System.Diagnostics.Debug.WriteLine("ERROR: " + message);
        }

        /// <summary>
        /// Print a debug error message from the Exception
        /// </summary>
        public static void Error(Exception ex)
        {
            if (DebugTraceListener != null)
                DebugTraceListener.WriteLine("ERROR: " + ex.ToString());
            else
                System.Diagnostics.Debug.WriteLine("ERROR: " + ex.ToString());
        }

        /// <summary>
        /// Print a debug warning message
        /// </summary>
        public static void Warning(string message)
        {
            if (DebugTraceListener != null)
                DebugTraceListener.WriteLine("WARN: " + message);
            else
                System.Diagnostics.Debug.WriteLine("WARN: " + message);
        }
    }
}
