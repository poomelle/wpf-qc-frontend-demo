using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Utilities
{
    public static class FileFolderUtility
    {
        /// <summary>
        /// Removes invalid characters from a file name.
        /// </summary>
        public static string SanitizeFileName(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        }

        /// <summary>
        /// Ensures a directory exists; creates it if it doesn't.
        /// </summary>
        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Combines path components and ensures a valid file path.
        /// </summary>
        public static string GetValidFilePath(string directory, string fileName)
        {
            string sanitizedFileName = SanitizeFileName(fileName);
            EnsureDirectoryExists(directory);
            return Path.Combine(directory, sanitizedFileName);
        }
    }
}
