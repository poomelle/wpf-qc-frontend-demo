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

        public static string FormatBatchRange(string fromBatch, string toBatch)
        {
            return fromBatch == toBatch ? fromBatch : $"{fromBatch}-{toBatch}";
        }

        public static string YearFromBatchName(string firstBatch)
        {
            return $"20{firstBatch.Substring(0, 2)}";
        }

        public static string RemoveTrailingAlphabetBatchName(string batchName)
        {
            batchName = batchName.TrimEnd("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
            return batchName;
        }
    }
}
