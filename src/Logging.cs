namespace SHRestAPI
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Provides static logging functionalities.
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// Logs a message with a specified verbosity level.
        /// </summary>
        /// <param name="level">The verbosity level of the log message.</param>
        /// <param name="message">The log message format string.</param>
        /// <param name="args">An object array that contains zero or more items to format.</param>
        public static void Log(VerbosityLevel level, string message, params object[] args)
        {
            Log(level, new Dictionary<string, string>(), message, args);
        }

        /// <summary>
        /// Logs a trace message.
        /// </summary>
        /// <param name="message">The log message format string.</param>
        /// <param name="args">An object array that contains zero or more items to format.</param>
        public static void LogTrace(string message, params object[] args)
        {
            Log(VerbosityLevel.Trivia, new Dictionary<string, string>(), message, args);
        }

        /// <summary>
        /// Logs a trace message with associated key-value pairs.
        /// </summary>
        /// <param name="values">A dictionary containing key-value pairs to be included in the log entry.</param>
        /// <param name="message">The log message format string.</param>
        /// <param name="args">An object array that contains zero or more items to format.</param>
        public static void LogTrace(IDictionary<string, string> values, string message, params object[] args)
        {
            Log(VerbosityLevel.Trivia, values, message, args);
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The log message format string.</param>
        /// <param name="args">An object array that contains zero or more items to format.</param>
        public static void LogInfo(string message, params object[] args)
        {
            Log(VerbosityLevel.SystemChatter, new Dictionary<string, string>(), message, args);
        }

        /// <summary>
        /// Logs an informational message with associated key-value pairs.
        /// </summary>
        /// <param name="values">A dictionary containing key-value pairs to be included in the log entry.</param>
        /// <param name="message">The log message format string.</param>
        /// <param name="args">An object array that contains zero or more items to format.</param>
        public static void LogInfo(IDictionary<string, string> values, string message, params object[] args)
        {
            Log(VerbosityLevel.SystemChatter, values, message, args);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The log message format string.</param>
        /// <param name="args">An object array that contains zero or more items to format.</param>
        public static void LogError(string message, params object[] args)
        {
            Log(VerbosityLevel.Significants, new Dictionary<string, string>(), message, args);
        }

        /// <summary>
        /// Logs an error message with associated key-value pairs.
        /// </summary>
        /// <param name="values">A dictionary containing key-value pairs to be included in the log entry.</param>
        /// <param name="message">The log message format string.</param>
        /// <param name="args">An object array that contains zero or more items to format.</param>
        public static void LogError(IDictionary<string, string> values, string message, params object[] args)
        {
            Log(VerbosityLevel.Significants, values, message, args);
        }

        /// <summary>
        /// Logs a message with a specified verbosity level and associated key-value pairs.
        /// </summary>
        /// <param name="level">The verbosity level of the log message.</param>
        /// <param name="values">A dictionary containing key-value pairs to be included in the log entry.</param>
        /// <param name="message">The log message format string.</param>
        /// <param name="args">An object array that contains zero or more items to format.</param>
        public static void Log(VerbosityLevel level, IDictionary<string, string> values, string message, params object[] args)
        {
            var sb = new StringBuilder("SHRestAPI: ");
            sb.AppendFormat("[{0}] ", level.ToString().ToUpperInvariant());
            sb.AppendFormat("DateTime={0} ", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            foreach (var key in values.Keys)
            {
                sb.AppendFormat("{0}={1} ", key, values[key]);
            }

            sb.Append("\n");
            foreach (var line in string.Format(message, args).Split("\n"))
            {
                sb.AppendFormat("    {0}\n", line);
            }

            // Roost.Birdsong.Tweet(level, 0, sb.ToString());
            NoonUtility.Log(sb.ToString());
        }
    }
}
