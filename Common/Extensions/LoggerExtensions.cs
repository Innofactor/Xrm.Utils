namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using Microsoft.Xrm.Sdk;

    public static class LoggerExtensions
    {
        private static readonly ConcurrentDictionary<int, int> indentations = new ConcurrentDictionary<int, int>();

        /// <summary>
        /// Starts a section in the log file
        /// </summary>
        /// <param name="name"></param>
        public static void StartSection(this ITracingService logger, string name)
        {
            var indentation = indentations.GetOrAdd(logger.GetHashCode(), 0);

            logger.Trace($"{Set(indentation)}↓ {name}");

            Interlocked.Increment(ref indentation);

            indentations.TryUpdate(logger.GetHashCode(), indentation, 0);
        }

        private static object Set(int indentation) =>
            new string(' ', (indentation > 0) ? indentation * 2 : 0);

        /// <summary>
        /// End section in the log file.
        /// </summary>
        public static void EndSection(this ITracingService logger)
        {
            var indentation = indentations.GetOrAdd(logger.GetHashCode(), 0);

            Interlocked.Decrement(ref indentation);

            logger.Trace($"{Set(indentation)}↑");

            indentations.TryUpdate(logger.GetHashCode(), indentation, 0);
        }

        public static void Log(this ITracingService logger, string message)
        {
            var indentation = indentations.GetOrAdd(logger.GetHashCode(), 0);

            logger.Trace("{0}{1}", Set(indentation), message);
        }

        /// <summary>
        /// Write message and parameter values to the log file.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Log(this ITracingService logger, string message, params object[] args) =>
            logger.Log(string.Format(message, args));

        public static void Log(this ITracingService logger, Exception ex)
        {
            var indentation = indentations.GetOrAdd(logger.GetHashCode(), 0);
            var padding = Set(indentation);

            logger.Trace("---------------------------------------------------------");
            logger.Trace("{0}{1}", padding, ex.ToString());
            logger.Trace("{0}{1}", padding, ex.Message);
            logger.Trace("{0}{1}", padding, ex.Source);
            logger.Trace("{0}{1}", padding, ex.StackTrace);
            logger.Trace("---------------------------------------------------------");
        }
    }
}