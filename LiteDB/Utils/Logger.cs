﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LiteDB
{
    /// <summary>
    /// A logger class to log all information about database. Used with levels. Level = 0 - 255
    /// All log will be trigger before operation execute (better for debug)
    /// </summary>
    public class Logger
    {
        public const byte NONE = 0;
        public const byte ERROR = 1;
        public const byte COMMAND = 2;
        public const byte QUERY = 4;
        public const byte WAL = 8;
        public const byte LOCK = 16;
        public const byte FULL = 255;

        /// <summary>
        /// Initialize logger class using a custom logging level (see Logger.NONE to Logger.FULL)
        /// </summary>
        public Logger(byte level = NONE, Action<string> logging = null)
        {
            this.Level = level;

            if (logging != null)
            {
                this.Logging += logging;
            }
        }

        /// <summary>
        /// Event when log writes a message. Fire on each log message
        /// </summary>
        public event Action<string> Logging = null;

        /// <summary>
        /// To full logger use Logger.FULL or any combination of Logger constants like Level = Logger.ERROR | Logger.COMMAND | Logger.DISK
        /// </summary>
        public byte Level { get; set; }

        public Logger()
        {
            this.Level = NONE;
        }

        internal void Error(string message)
        {
            this.Write(ERROR, message);
        }

        /// <summary>
        /// Execute msg function only if level are enabled
        /// </summary>
        public void Write(byte level, Func<string> fn)
        {
            if ((level & this.Level) == 0) return;

            this.Write(level, fn());
        }

        /// <summary>
        /// Write log text to output using inside a component (statics const of Logger)
        /// </summary>
        public void Write(byte level, string message, params object[] args)
        {
            if ((level & this.Level) == 0 || string.IsNullOrEmpty(message)) return;

            if (this.Logging != null)
            {
                var text = string.Format(message, args);

                var str =
                    level == ERROR ? "ERROR" :
                    level == COMMAND ? "COMMAND" :
                    level == QUERY ? "QUERY" :
                    level == LOCK ? "LOCK" :
                    level == WAL ? "WAL" : "";

                var msg = Task.CurrentId.ToString().PadLeft(1, '0') + " [" + str + "] " + text;

                try
                {
                    this.Logging(msg);
                }
                catch
                {
                }
            }
        }
    }
}