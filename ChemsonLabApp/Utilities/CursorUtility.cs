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
        /// <summary>
        /// Sets the mouse cursor to a wait cursor if processing is true, or restores the default cursor if false.
        /// </summary>
        /// <param name="processing">If true, sets the cursor to wait; otherwise, restores the default cursor.</param>
        public static void DisplayCursor(bool processing)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = processing ? Cursors.Wait : null;
            });
        }
    }
}
