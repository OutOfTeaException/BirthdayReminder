using System;
using System.IO;

namespace BirthdayReminder.Util
{
    public static class Log
    {
        private static string documentsPath;
        private static string logFileName;
        private static string logFile;

        public static void Debug(string message)
        {
            Write("DEBUG", message);
        }

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Error(string message)
        {
            Write("ERROR", message);
        }

        private static void Write(string level, string message)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {level:-5} - {message}";
                
            File.AppendAllText(logFile, logMessage);
        }

        static Log()
        {
            documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            logFileName = "log.txt";
            logFile = Path.Combine(documentsPath, logFileName);

            Directory.CreateDirectory(documentsPath);
        }
    }
}