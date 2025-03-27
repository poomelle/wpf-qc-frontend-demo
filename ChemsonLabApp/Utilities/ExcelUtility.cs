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
