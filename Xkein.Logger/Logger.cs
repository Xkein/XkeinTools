using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Xkein
{
    /// <summary>Represents the log behavior.</summary>
    public class Logger
    {
        private static Lazy<Logger> _default = new Lazy<Logger>(false);
        /// <summary>Get default Logger.</summary>
        public static Logger Default => _default.Value;

        /// <summary>Represents the method that will handle log message.</summary>
        public delegate void WriteLineDelegate(string str);
        /// <summary>Invoked when log the message.</summary>
        public WriteLineDelegate WriteLine { get; set; }
        /// <summary>Write format string to logger.</summary>
        public void Log(string format, params object[] args)
        {
            string str = string.Format(format, args);

            Log(str);
        }

        /// <summary>Write string to logger.</summary>
        public void Log(string str)
        {
            WriteLine.Invoke(str);
        }

        /// <summary>Write object to logger.</summary>
        public void Log(object obj)
        {
            Log(obj.ToString());
        }

        /// <summary>Get if PrintException has invoked.</summary>
        public bool HasException { get; set; } = false;

        /// <summary>Print exception and its InnerException recursively.</summary>
        public void PrintException(Exception e)
        {
            HasException = true;

            PrintExceptionBase(e);
            Log("");
        }

        private void PrintExceptionBase(Exception e)
        {
            LogError("{0} info: ", e.GetType().FullName);
            LogError("Message: " + e.Message);
            LogError("Source: " + e.Source);
            LogError("TargetSite.Name: " + e.TargetSite?.Name);
            LogError("Stacktrace: " + e.StackTrace);

            if (e.InnerException != null)
            {
                PrintException(e.InnerException);
            }
        }

        private object color_locker = new object();
        /// <summary>Write string to logger with color.</summary>
        public void LogWithColor(string str, ConsoleColor color)
        {
            LogWithColor(str, color, Console.BackgroundColor);
        }

        /// <summary>Write string to logger with ForegroundColor and BackgroundColor.</summary>
        public void LogWithColor(string str, ConsoleColor fgColor, ConsoleColor bgColor)
        {
            lock (color_locker)
            {
                ConsoleColor originFgColor = Console.ForegroundColor;
                ConsoleColor originBgColor = Console.BackgroundColor;
                Console.ForegroundColor = fgColor;
                Console.BackgroundColor = bgColor;

                Log(str);

                Console.ForegroundColor = originFgColor;
                Console.BackgroundColor = originBgColor;
            }
        }

        /// <summary>Write format string to logger with error state.</summary>
        public void LogError(string format, params object[] args)
        {
            string str = string.Format(format, args);

            LogError(str);
        }

        /// <summary>Write string to logger with error state.</summary>
        public void LogError(string str)
        {
            LogWithColor("[Error] " + str, ConsoleColor.Red);
        }

        /// <summary>Write object to logger with error state.</summary>
        public void LogError(object obj)
        {
            LogError(obj.ToString());
        }

        /// <summary>Write format string to logger with warning state.</summary>
        public void LogWarning(string format, params object[] args)
        {
            string str = string.Format(format, args);

            LogWarning(str);
        }

        /// <summary>Write string to logger with warning state.</summary>
        public void LogWarning(string str)
        {
            LogWithColor("[Warning] " + str, ConsoleColor.Yellow);
        }

        /// <summary>Write object to logger with warning state.</summary>
        public void LogWarning(object obj)
        {
            LogWarning(obj.ToString());
        }

        /// <summary>Create file to log.</summary>
        public void CreateFile(string path)
        {
            FileStream logFileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            CreateStreamWriter(logFileStream);
        }

        /// <summary>Create writer to log.</summary>
        public void CreateStreamWriter(Stream stream)
        {
            var writer = new StreamWriter(stream);
            writer.AutoFlush = true;

            WriteLine += (string str) =>
            {
                writer.WriteLine(str);
            };
        }
    }
}
