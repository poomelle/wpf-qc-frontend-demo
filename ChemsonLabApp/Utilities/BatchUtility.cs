using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace ChemsonLabApp.Utilities
{
    public static class BatchUtility
    {
        /// <summary>
        /// Generates a list of batch names from the specified starting batch to the ending batch.
        /// The batches must share the same year and month prefix (first 3 characters).
        /// Returns an empty list if the input is invalid or the prefixes do not match.
        /// </summary>
        /// <param name="fromBatch">The starting batch name (e.g., "24001").</param>
        /// <param name="toBatch">The ending batch name (e.g., "24010").</param>
        /// <returns>A list of batch names from fromBatch to toBatch, inclusive.</returns>
        public static List<string> GenerateBatchNameFromTo(string fromBatch, string toBatch)
        {
            List<string> batches = new List<string>();

            if (string.IsNullOrWhiteSpace(fromBatch) || string.IsNullOrWhiteSpace(toBatch)) return batches;

            string fromYearMonthBatch = fromBatch.Length >= 3 ? fromBatch.Substring(0, 3) : null;
            string toYearMonthBatch = toBatch.Length >= 3 ? toBatch.Substring(0, 3) : null;

            if (fromYearMonthBatch == null || toYearMonthBatch == null || fromYearMonthBatch != toYearMonthBatch)
            {
                return batches;
            }

            if (int.TryParse(fromBatch.Substring(3), out int fromBatchNumber) &&
                int.TryParse(toBatch.Substring(3), out int toBatchNumber))
            {
                if (fromBatchNumber > toBatchNumber)
                {
                    NotificationUtility.ShowError("From batch number should be less than or equal to To batch number.");
                    return batches;
                }

                for (int i = fromBatchNumber; i <= toBatchNumber; i++)
                {
                    batches.Add($"{fromYearMonthBatch}{i}");
                }
            }

            return batches;
        }

        /// <summary>
        /// Formats a batch range as a string. If the fromBatch and toBatch are the same, returns the single batch name.
        /// Otherwise, returns a string in the format "fromBatch-toBatch".
        /// </summary>
        public static string FormatBatchRange(string fromBatch, string toBatch)
        {
            return fromBatch == toBatch ? fromBatch : $"{fromBatch}-{toBatch}";
        }

        /// <summary>
        /// Extracts the year from a batch name by taking the first two characters and prefixing with "20".
        /// </summary>
        public static string YearFromBatchName(string firstBatch)
        {
            return $"20{firstBatch.Substring(0, 2)}";
        }

        /// <summary>
        /// Removes any trailing alphabetic characters from the end of a batch name.
        /// </summary>
        public static string RemoveTrailingAlphabetBatchName(string batchName)
        {
            batchName = batchName.TrimEnd("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
            return batchName;
        }
    }
}
