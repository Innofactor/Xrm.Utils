namespace Innofactor.Xrm.Utils.Common.Loggers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using Innofactor.Xrm.Utils.Common.Interfaces;

    /// <summary>File based logger</summary>
    public class FileLogger : ILoggable
    {
        #region Public Fields

        /// <summary>
        /// Timestamp constant string
        /// </summary>
        public const string TIMESTAMP = "[TIMESTAMP]";

        #endregion Public Fields

        #region Protected Fields

        /// <summary>
        ///
        /// </summary>
        protected List<Tuple<string, DateTime>> stack = new List<Tuple<string, DateTime>>();

        #endregion Protected Fields

        #region Private Fields

        private DateTime lastLog = DateTime.MinValue;
        private TextWriter twLog = default;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>Constructor for the Logging class</summary>
        /// <param name="fullName">Full file path (folder+filename), defaults to C:\Temp\"AssemblyName"</param>
        public FileLogger(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = $@"C:\Temp\{Assembly.GetExecutingAssembly().GetName().Name}";
            }

            twLog = CreateFile(fullName);
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>Name of current log file</summary>
        public string FileName { get; private set; } = "";

        #endregion Public Properties

        #region Protected Properties

        /// <summary>Bool indicating if plugin context logging is activated</summary>
        protected bool LogContext { get; set; }

        /// <summary>Bool indicating if entity logging is activated</summary>
        protected bool LogEntity { get; set; }

        /// <summary>Path to log file</summary>
        protected string LogPath { get; set; }

        /// <summary>Name of consuming assembly</summary>
        protected MethodBase Origin { get; set; }

        #endregion Protected Properties

        #region Public Methods

        /// <summary>Closes the log file</summary>
        public virtual void CloseLog() => CloseLog("");

        /// <summary>Closes the log file after writing the closetext</summary>
        /// <param name="closetext">Text to write before closing</param>
        public virtual void CloseLog(string closetext)
        {
            if (twLog != null)
            {
                if (!string.IsNullOrEmpty(closetext))
                {
                    Log("-----> " + closetext + "\n");
                }

                FileName = string.Empty;
                twLog.Close();
                twLog = null;
            }
        }

        /// <summary>Removes last section from the stack and decreases indentation</summary>
        public virtual void EndSection()
        {
            int i = stack.Count - 1;
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

        /// <summary>Appends timestamp and "s" to the log file</summary>
        /// <param name="s">String to write to log file</param>
        public virtual void Log(string s)
        {
            s = ComposeLogText(s, true);
            WriteToLog(s);
        }

        /// <summary>Log formatted string</summary>
        /// <param name="s">Format string to write to log file</param>
        /// <param name="arg">Arguments to the format string</param>
        public virtual void Log(string s, params object[] arg) => Log(string.Format(CultureInfo.InvariantCulture, s, arg));

        /// <summary>Log Exception</summary>
        /// <param name="ex">Exception to log</param>
        public virtual void Log(Exception ex)
        {
            Log(TIMESTAMP);
            WriteToLog($"*** Exception:\n{ex.ToString()}");
            LogException(ex);
        }

        /// <summary>Appends timestamp and "s" to the log file</summary>
        /// <param name="log">bool to test i write to log</param>
        /// <param name="s">String to write to log file</param>
        public virtual void LogIf(bool log, string s)
        {
            if (log)
            {
                Log(s);
            }
        }

        /// <summary>Log formatted string</summary>
        /// <param name="log">bool to test i write to log</param>
        /// <param name="s">Format string to write to log file</param>
        /// <param name="arg">Arguments to the format string</param>
        public virtual void LogIf(bool log, string s, params object[] arg)
        {
            if (log)
            {
                Log(s, arg);
            }
        }

        /// <summary>Performs custom rough parsing of an XML parentNode</summary>
        /// <param name="node">Node to parse</param>
        /// <param name="indent">Initial indentation for this parentNode</param>
        public void ParseNode(XmlNode node, string indent)
        {
            Log(indent + node.NodeType.ToString() + ": " + node.Name);
            Log(indent + " // " + node.InnerXml.Replace("\n", "<BR>"));
            if (node.Attributes != null)
            {
                foreach (XmlAttribute attr in node.Attributes)
                {
                    Log(indent + "  " + attr.NodeType.ToString() + ": " + attr.Name + "=" + attr.Value);
                }
            }
            foreach (XmlNode child in node.ChildNodes)
            {
                ParseNode(child, indent + "  ");
            }
        }

        /// <summary>Add a new section to the stack. Increases indentation of log lines until next EndSection call</summary>
        /// <param name="section">Section name</param>
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

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        ///
        /// </summary>
        /// <param name="s"></param>
        /// <param name="indent"></param>
        /// <returns></returns>
        protected string ComposeLogText(string s, bool indent)
        {
            var now = DateTime.Now;
            var startstring = string.Empty;
            if ((!lastLog.Equals(now) && !lastLog.Equals(DateTime.MinValue)) || s.Equals(TIMESTAMP))
            {
                var span = lastLog.Equals(DateTime.MinValue) ? 0 : (now - lastLog).TotalMilliseconds;
                startstring = string.Format("{0:0}", span);
            }
            if (indent)
            {
                startstring += $"\t{Indentchars()}";
                s = s.Replace("\n", "\n\t");
            }
            lastLog = now;
            return startstring + s.Replace(TIMESTAMP, "");
        }

        /// <summary>
        /// Returns current indentation based on section stack count
        /// </summary>
        /// <returns></returns>
        protected string Indentchars()
        {
            return new string(' ', (stack.Count > 0) ? stack.Count * 2 : 0); ;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="s"></param>
        protected virtual void WriteToLog(string s)
        {
            if (twLog != null)
            {
                try
                {
                    twLog.WriteLine(s);
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private TextWriter CreateFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(path, "path must be a full file path including name");
            }
            TextWriter tw = null;

            string filename = Path.GetFileName(path);
            string folderPath = Path.GetDirectoryName(path);

            try
            {   // First attempt
                tw = TextWriter.Synchronized(File.AppendText(path));
            }
            catch
            {
                int i = 1;
                bool ok = false;
                string extension = Path.GetExtension(path);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                while (i < 10 && !ok)
                {
                    try
                    {   // Second attempt, append i to file name
                        var newFileName = fileNameWithoutExtension + i + extension;
                        filename = Path.Combine(folderPath, newFileName);
                        tw = TextWriter.Synchronized(File.AppendText(filename));
                        ok = true;
                    }
                    catch
                    {
                        i++;
                    }
                }

                if (i >= 10)
                {   // Logging seems troublesome today. Never mind. Byegones.
                    tw = null;
                    filename = "";
                }
            }
            FileName = filename;
            return tw;
        }

        private MethodBase GetOrigin()
        {
            StackFrame[] stackFrames = new StackTrace(true).GetFrames();
            MethodBase mb = null;
            try
            {
                foreach (StackFrame stackFrame in stackFrames)
                {
                    mb = stackFrame.GetMethod();
                    if (!mb.ReflectedType.FullName.ToLowerInvariant().StartsWith("innofactor.xrm.utils"))
                    {
                        break;
                    }
                }
            }
            catch (Exception ex) { }
            return mb;
        }

        private void LogException(Exception ex)
        {
            var padding = Indentchars();
            WriteToLog("---------------------------------------------------------");
            WriteToLog($"{padding}{ex.ToString()}");
            WriteToLog($"{padding}{ex.Message}");
            WriteToLog($"{padding}{ex.Source}");
            WriteToLog($"{padding}{ex.StackTrace}");
            WriteToLog("---------------------------------------------------------");
        }

        #endregion Private Methods
    }
}