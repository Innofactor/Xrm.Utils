namespace Innofactor.Xrm.Utils.Common.Loggers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;

    /// <summary>Class to handle logging functionality for CRM using the ITracingService for logging</summary>
    public class CRMLogger : ILoggable
    {
        /// <summary>
        ///
        /// </summary>
        protected ITracingService tracingService;
        /// <summary>
        /// Keeps track of the section stack in the log
        /// </summary>
        protected List<Tuple<string, DateTime>> stack = new List<Tuple<string, DateTime>>();

        /// <summary>
        ///
        /// </summary>
        public CRMLogger(ITracingService tracingService)
        {
            this.tracingService = tracingService;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ex"></param>
        public virtual void Log(Exception ex)
        {
            var padding = Indentchars();

            tracingService.Trace("---------------------------------------------------------");
            tracingService.Trace("{0}{1}", padding, ex.ToString());
            tracingService.Trace("{0}{1}", padding, ex.Message);
            tracingService.Trace("{0}{1}", padding, ex.Source);
            tracingService.Trace("{0}{1}", padding, ex.StackTrace);
            tracingService.Trace("---------------------------------------------------------");
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        public virtual void Log(string message, params object[] arg) => Log(string.Format(CultureInfo.InvariantCulture, message, arg));

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        public virtual void Log(string message)
        {
            WriteToLog(ComposeLogText(message, true));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        protected void WriteToLog(string message)
        {
            if (tracingService != null)
            {
                tracingService.Trace(message);
            }
        }
        /// <summary>Removes last section from the stack and decreases indentation</summary>
        public virtual void EndSection()
        {
            var i = stack.Count - 1;
            try
            {
                var section = stack[i];
                stack.RemoveAt(i);
                Log("↑ {0} ({1:0})", section.Item1, (DateTime.Now - section.Item2).TotalMilliseconds);
            }
            catch (ArgumentOutOfRangeException)
            {
                Log("  *** Logger: Invalid section stack index: {0}", i);
                Log("↑ -unknown section-");
            }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="section"></param>
        public virtual void StartSection(string section = null)
        {
            if (string.IsNullOrEmpty(section))
            {
                var mb = GetOrigin();
                section = mb.ReflectedType.Name + "." + mb.Name;
            }
            Log("↓ {0}", section);
            stack.Add(new Tuple<string, DateTime>(section, DateTime.Now));
        }
        private MethodBase GetOrigin()
        {
            var stackFrames = new StackTrace(true).GetFrames();
            MethodBase mb = null;
            try
            {
                foreach (var stackFrame in stackFrames)
                {
                    mb = stackFrame.GetMethod();
                    if (!mb.ReflectedType.FullName.ToLowerInvariant().StartsWith("innofactor.xrm.utils"))
                    {
                        break;
                    }
                }
            }
            catch { }
            return mb;
        }
        /// <summary>Closes the log file</summary>
        public virtual void CloseLog() => CloseLog("");

        /// <summary>Closes the log file after writing the closetext</summary>
        /// <param name="closetext">Text to write before closing</param>
        public virtual void CloseLog(string closetext)
        {
            if (!string.IsNullOrEmpty(closetext))
            {
                Log("-----> " + closetext + "\n");
            }
        }
        /// <summary>
        /// Creates string to log, based on section stack count, delta time since last log, and actual text to log
        /// </summary>
        /// <param name="text"></param>
        /// <param name="indent"></param>
        /// <returns></returns>
        protected string ComposeLogText(string text, bool indent)
        {
            var startstring = string.Empty;

            if (indent)
            {
                startstring += $"\t{Indentchars()}";
                text = text.Replace("\n", "\n\t");
            }

            return startstring + text;
        }
        /// <summary>
        /// Returns current indentation based on section stack count
        /// </summary>
        /// <returns></returns>
        protected string Indentchars()
        {
            return new string(' ', (stack.Count > 0) ? stack.Count * 2 : 0); ;
        }

        private class AttributeSorter : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x.Contains(".") == y.Contains("."))
                {   // Both contain or does not contain dot indicating aliased/linked attribute
                    return string.Compare(x, y, StringComparison.InvariantCulture);
                }
                if (y.Contains("."))
                {
                    return -1;
                }
                return 1;
            }
        }
    }
}