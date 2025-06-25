using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.DataLoaderService
{
    public class SearchDataLoaderService : ISearchDataLoaderService
    {
        private readonly IBatchTestResultRestAPI _batchTestResultRestAPI;

        public SearchDataLoaderService(IBatchTestResultRestAPI batchTestResultRestAPI)
        {
            this._batchTestResultRestAPI = batchTestResultRestAPI;
        }

        /// <summary>
        /// Loads BCH batch test results based on the provided search criteria.
        /// If only fromBatchName is provided, loads results for that batch.
        /// If both fromBatchName and toBatchName are provided, loads results for all batches in the range.
        /// If neither is provided, loads results by test date.
        /// Filters results by the specified suffix.
        /// </summary>
        public async Task<List<BatchTestResult>> LoadBCHBatchTestResult(string productName, string testNumber, string suffix, string fromBatchName, string toBatchName, DateTime testDate)
        {
            var loadedBatchTestResults = new List<BatchTestResult>();
            if (!string.IsNullOrWhiteSpace(fromBatchName) && string.IsNullOrWhiteSpace(toBatchName))
            {
                var batchTestResultFilter = $"?productName={productName}&exactBatchName={fromBatchName}&testNumber={testNumber}";
                var batchTestResults = await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(filter: batchTestResultFilter);
                if (batchTestResults.Count() > 0)
                {
                    batchTestResults = batchTestResults.Where(x => x.batch.suffix == suffix).ToList();
                    loadedBatchTestResults.AddRange(batchTestResults);
                }
            }
            else if (!string.IsNullOrWhiteSpace(fromBatchName) && !string.IsNullOrWhiteSpace(toBatchName))
            {
                List<string> batches = new List<string>();
                string yearMonth = fromBatchName.Substring(0, 3);
                int fromBatch = int.Parse(fromBatchName.Substring(3));
                int toBatch = int.Parse(toBatchName.Substring(3));

                for (int i = fromBatch; i <= toBatch; i++)
                {
                    string batch = $"{yearMonth}{i}";
                    batches.Add(batch);
                }

                if (batches.Count() > 0)
                {
                    foreach (string batchName in batches)
                    {
                        var batchTestResultFilter = $"?productName={productName}&exactBatchName={batchName}&testNumber={testNumber}";
                        var batchTestResults = await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(filter: batchTestResultFilter);
                        if (batchTestResults.Count() > 0)
                        {
                            batchTestResults = batchTestResults.Where(x => x.batch.suffix == suffix).ToList();
                            loadedBatchTestResults.AddRange(batchTestResults);
                        }
                    }
                }
            }
            else
            {
                var testDateString = testDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var batchTestResultFilter = $"?productName={productName}&testDate={testDateString}";
                var batchTestResults = await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(filter: batchTestResultFilter);

                if (batchTestResults.Count() > 0)
                {
                    batchTestResults = batchTestResults.Where(x => x.batch.suffix == suffix).ToList();
                    loadedBatchTestResults.AddRange(batchTestResults);
                }
            }

            return loadedBatchTestResults;
        }

        /// <summary>
        /// Loads batch test results for WarmUp and STD test types based on BCH batch test results.
        /// Uses the product name and batch group from the first BCH batch test result.
        /// </summary>
        public async Task<List<BatchTestResult>> LoadWarmUpAndSTDBatchTestResult(List<BatchTestResult> bchBatchTestResults, string testType)
        {
            var productName = bchBatchTestResults.FirstOrDefault().testResult.product.name;
            var batchGroupName = bchBatchTestResults.FirstOrDefault().testResult.batchGroup;

            var batchFilter = $"?productName={productName}&batchGroup={batchGroupName}&exactBatchName={testType}";

            if (testType == "STD")
            {
                batchFilter = $"?productName={productName}&batchGroup={batchGroupName}&batchName={testType}";
            }

            return await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(filter: batchFilter);
        }
    }
}
