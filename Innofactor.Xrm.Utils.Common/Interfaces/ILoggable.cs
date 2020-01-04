namespace Innofactor.Xrm.Utils.Common.Interfaces
{
    using System;

    /// <summary>
    /// Interface for Logging implementations
    /// </summary>
    public interface ILoggable
    {
        /// <summary>
        /// 
        /// </summary>
        void CloseLog();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="closetext"></param>
        void CloseLog(string closetext);

        /// <summary>
        /// Log the end of a section
        /// </summary>
        void EndSection();

        /// <summary>
        /// Log any string based message
        /// </summary>
        /// <param name="message"></param>
        void Log(string message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        void Log(Exception ex);
        /// <summary>
        /// Log any string based message along with parameters
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        void Log(string message, params object[] arg);

        /// <summary>
        /// Log the start of a section
        /// </summary>
        /// <param name="name"></param>
        void StartSection(string name = null);
    }
}