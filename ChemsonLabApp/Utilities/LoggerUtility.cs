using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Utilities
{
    public static class LoggerUtility
    {
        private static readonly string logFilePath = "D:\\Projects\\ChemsonQC\\Frontend_WPF\\LabAppLog.txt";

        /// <summary>
        /// Logs error details including message and stack trace to a log file.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        public static void LogError(Exception ex)
        {
            try
            {
                string logMessage = $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n";
                File.AppendAllText(logFilePath, logMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while logging: {e.Message}");
            }
        }
    }
}
