using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ChemsonLabApp.Utilities
{
    public static class InputValidationUtility
    {
        /// <summary>
        /// Validates if the selected product is not null.
        /// </summary>
        public static bool ValidateNotNullObject(object selected, string message)
        {
            if (selected == null)
            {
                NotificationUtility.ShowError($"Please select {message}.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if the batch number input is not empty or whitespace.
        /// </summary>
        public static bool ValidateNotNullInput(string batchNumber, string message)
        {

            if (string.IsNullOrWhiteSpace(batchNumber))
            {
                NotificationUtility.ShowError($"Please input {message}.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the batch number format:
        /// - First two characters must be numbers (0-9)
        /// - Third character must be A-M (excluding 'I')
        /// - Remaining characters must be numbers (0-9)
        /// </summary>
        public static bool ValidateBatchNumberFormat(string batchNumber)
        {
            string pattern = @"^[0-9]{2}[A-HJ-Ma-hj-m][0-9]*$";

            if (!Regex.IsMatch(batchNumber, pattern))
            {
                NotificationUtility.ShowError("Invalid batch number format. ex: 25A1");
                return false;
            }

            return true;
        }

        public static bool ValidateEmailFormat(string email)
        {
            var rexPattern = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (!rexPattern.IsMatch(email))
            {
                NotificationUtility.ShowError("Invalid email format.");
                return false;
            }

            return true;

        }

        public static bool DeleteConfirmation(string message)
        {
            if (message != "DELETE")
            {
                NotificationUtility.ShowError("Please type 'DELETE' to confirm deletion.");
                return false;
            }

            return true;
        }
    }
}
