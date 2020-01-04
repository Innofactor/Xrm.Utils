namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System;
    using Innofactor.Xrm.Utils.Common.Interfaces;

    public static partial class ContainerExtensions
    {
        #region Public Methods

        /// <summary>
        /// End section in the log file.
        /// </summary>
        public static void EndSection(this IExecutionContainer container) =>
           container.Logger.EndSection();

        /// <summary>
        /// Log a string message
        /// </summary>
        /// <param name="container"></param>
        /// <param name="message"></param>
        public static void Log(this IExecutionContainer container, string message) =>
            container.Logger.Log(message);

        /// <summary>
        /// Write message and parameter values to the log file.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Log(this IExecutionContainer container, string message, params object[] args) =>
            container.Logger.Log(message, args);

        /// <summary>
        ///
        /// </summary>
        /// <param name="container"></param>
        /// <param name="ex"></param>
        public static void Log(this IExecutionContainer container, Exception ex) => 
            container.Logger.Log(ex);

        /// <summary>
        /// Starts a section in the log file
        /// </summary>
        /// <param name="container"></param>
        /// <param name="name"></param>
        public static void StartSection(this IExecutionContainer container, string name) =>
            container.Logger.StartSection(name);

        #endregion Public Methods
    }
}