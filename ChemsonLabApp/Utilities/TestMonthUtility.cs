using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Utilities
{
    public static class TestMonthUtility
    {
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
