namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using Innofactor.Xrm.Utils.Common.Interfaces;

    public static partial class ContainerExtensions
    {
        private static readonly ConcurrentDictionary<int, int> indentations = new ConcurrentDictionary<int, int>();

        /// <summary>
        /// Starts a section in the log file
        /// </summary>
        /// <param name="name"></param>
        public static void StartSection(this IExecutionContainer container, string name)
        {
            var indentation = indentations.GetOrAdd(container.GetHashCode(), 0);

            container.Logger.Trace($"{Set(indentation)}↓ {name}");

            Interlocked.Increment(ref indentation);

            indentations.TryUpdate(container.GetHashCode(), indentation, 0);
        }

        private static object Set(int indentation) =>
            new string(' ', (indentation > 0) ? indentation * 2 : 0);

        /// <summary>
        /// End section in the log file.
        /// </summary>
        public static void EndSection(this IExecutionContainer container)
        {
            var indentation = indentations.GetOrAdd(container.GetHashCode(), 0);

            Interlocked.Decrement(ref indentation);

            container.Logger.Trace($"{Set(indentation)}↑");

            indentations.TryUpdate(container.GetHashCode(), indentation, 0);
        }

        public static void Log(this IExecutionContainer container, string message)
        {
            var indentation = indentations.GetOrAdd(container.GetHashCode(), 0);

            container.Logger.Trace("{0}{1}", Set(indentation), message);
        }

        /// <summary>
        /// Write message and parameter values to the log file.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Log(this IExecutionContainer container, string message, params object[] args) =>
            container.Log(string.Format(message, args));

        public static void Log(this IExecutionContainer container, Exception ex)
        {
            var indentation = indentations.GetOrAdd(container.GetHashCode(), 0);
            var padding = Set(indentation);

            container.Logger.Trace("---------------------------------------------------------");
            container.Logger.Trace("{0}{1}", padding, ex.ToString());
            container.Logger.Trace("{0}{1}", padding, ex.Message);
            container.Logger.Trace("{0}{1}", padding, ex.Source);
            container.Logger.Trace("{0}{1}", padding, ex.StackTrace);
            container.Logger.Trace("---------------------------------------------------------");
        }
    }
}