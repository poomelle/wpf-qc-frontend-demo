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

        public static void LogError(Exception ex)
        {
            try
            {
                string logMessage = $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n";
                File.AppendAllText(logFilePath, logMessage);

            }catch (Exception e)
            {
                Console.WriteLine($"Error while logging: {e.Message}");
            }
        }
    }
}
