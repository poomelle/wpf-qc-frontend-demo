using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Utilities
{
    public static class ExcelUtility
    {
        /// <summary>
        /// Opens a file dialog for the user to select an Excel file and returns the selected file path.
        /// </summary>
        /// <returns>The full path of the selected Excel file, or an empty string if no file was selected.</returns>
        public static string GetExcelFilePath()
        {
            string filePath = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }
            return filePath;
        }
    }
}
