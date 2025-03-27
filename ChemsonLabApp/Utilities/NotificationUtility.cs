using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChemsonLabApp.Utilities
{
    public static class NotificationUtility
    {
        /// <summary>
        /// Displays a success message.
        /// </summary>
        public static void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Displays an error message.
        /// </summary>
        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Displays a warning message.
        /// </summary>
        public static void ShowWarning(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Displays a confirmation dialog and returns true if user clicks 'Yes'.
        /// </summary>
        public static bool ShowConfirmation(string message)
        {
            return MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
