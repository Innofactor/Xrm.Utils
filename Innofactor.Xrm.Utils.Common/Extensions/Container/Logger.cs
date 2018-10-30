namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System;
    using System.Threading;
    using Innofactor.Xrm.Utils.Common.Interfaces;

    public static partial class ContainerExtensions
    {
        #region Public Methods

        /// <summary>
        /// End section in the log file.
        /// </summary>
        public static void EndSection(this IExecutionContainer container)
        {
            var indentation = (int)container?.Values?.IndentationLevel;

            Interlocked.Decrement(ref indentation);

            container.Logger.Trace($"{Set(indentation)}↑");

            container.Values.IndentationLevel = indentation;
        }

        public static void Log(this IExecutionContainer container, string message)
        {
            var indentation = (int)container?.Values?.IndentationLevel;

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
            var indentation = (int)container?.Values?.IndentationLevel;

            var padding = Set(indentation);

            container.Logger.Trace("---------------------------------------------------------");
            container.Logger.Trace("{0}{1}", padding, ex.ToString());
            container.Logger.Trace("{0}{1}", padding, ex.Message);
            container.Logger.Trace("{0}{1}", padding, ex.Source);
            container.Logger.Trace("{0}{1}", padding, ex.StackTrace);
            container.Logger.Trace("---------------------------------------------------------");
        }

        /// <summary>
        /// Starts a section in the log file
        /// </summary>
        /// <param name="name"></param>
        public static void StartSection(this IExecutionContainer container, string name)
        {
            var indentation = (int)container?.Values?.IndentationLevel;

            container.Logger.Trace($"{Set(indentation)}↓ {name}");

            Interlocked.Increment(ref indentation);

            container.Values.IndentationLevel = indentation;
        }

        #endregion Public Methods

        #region Private Methods

        private static object Set(int indentation) =>
            new string(' ', (indentation > 0) ? indentation * 2 : 0);

        #endregion Private Methods
    }
}