using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Utilities
{
    public static class TestMonthUtility
    {
        /// <summary>
        /// Returns the full month name corresponding to the last character of the given batch group name.
        /// The last character is expected to be a letter code ('a' to 'm') representing months January to December.
        /// Returns an empty string if the character does not match any month code.
        /// </summary>
        public static string TestMonth(string batchGroupName)
        {
            char monthChar = batchGroupName[batchGroupName.Length - 1];
            switch (char.ToLower(monthChar))
            {
                case 'a':
                    return "January";
                case 'b':
                    return "February";
                case 'c':
                    return "March";
                case 'd':
                    return "April";
                case 'e':
                    return "May";
                case 'f':
                    return "June";
                case 'g':
                    return "July";
                case 'h':
                    return "August";
                case 'j':
                    return "September";
                case 'k':
                    return "October";
                case 'l':
                    return "November";
                case 'm':
                    return "December";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Converts a single-character month code to its three-letter abbreviation, or vice versa.
        /// If the input is a valid code ('a' to 'm'), returns the corresponding month abbreviation ("Jan" to "Dec").
        /// If the input is a valid month abbreviation ("Jan" to "Dec"), returns the corresponding code in uppercase.
        /// Returns an empty string for invalid input.
        /// </summary>
        public static string ConvertMonth(string input)
        {
            var charToMonthMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "a", "Jan" },
                { "b", "Feb" },
                { "c", "Mar" },
                { "d", "Apr" },
                { "e", "May" },
                { "f", "Jun" },
                { "g", "Jul" },
                { "h", "Aug" },
                { "j", "Sep" },
                { "k", "Oct" },
                { "l", "Nov" },
                { "m", "Dec" }
            };

            if (charToMonthMap.TryGetValue(input, out string month))
            {
                return month;
            }
            else
            {
                foreach (var kvp in charToMonthMap)
                {
                    if (kvp.Value.Equals(input, StringComparison.OrdinalIgnoreCase))
                    {
                        return kvp.Key.ToUpper();
                    }
                }
            }

            return "";
        }
    }
}
