using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChemsonLabApp.Utilities
{
    public static class CursorUtility
    {
        public static void DisplayCursor(bool processing)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = processing ? Cursors.Wait : null;
            });
        }
    }
}
