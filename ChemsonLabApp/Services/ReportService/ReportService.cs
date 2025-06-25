using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRestAPI _reportRestAPI;
        private readonly ITestResultReportRestAPI _testResultReportRestAPI;
        private readonly IBatchTestResultRestAPI _batchTestResultRestAPI;
        private readonly IEvaluationRestAPI _evaluationRestAPI;

        public ReportService(
            IReportRestAPI reportRestAPI,
            ITestResultReportRestAPI testResultReportRestAPI,
            IBatchTestResultRestAPI batchTestResultRestAPI,
            IEvaluationRestAPI evaluationRestAPI)
        {
            this._reportRestAPI = reportRestAPI;
            this._testResultReportRestAPI = testResultReportRestAPI;
            this._batchTestResultRestAPI = batchTestResultRestAPI;
            this._evaluationRestAPI = evaluationRestAPI;
        }

        /// <summary>
        /// Calculates the average test time in ticks for a list of batch test results.
        /// Only considers test results from the most recent test date and intervals less than 30 minutes.
        /// Returns 0 if the input list is empty.
        /// </summary>
        /// <param name="batchTestResults">List of BatchTestResult objects to calculate the average test time from.</param>
        /// <returns>The average test time in ticks as a long value.</returns>
        public long CalculateAveTestTimeTick(List<BatchTestResult> batchTestResults)
        {
            long aveTestTimeTick = 0;

            if (batchTestResults.Count == 0)
            {
                NotificationUtility.ShowWarning("No batch test results to calculate average test time tick.");
                return aveTestTimeTick;
            }

            TimeSpan totalRunTime = TimeSpan.Zero;
            string format = "dd/MM/yyyy HH:mm";

            var sortedBatchTestResults = batchTestResults.OrderBy(result => result.testResult.testDate).ToList();

            DateTime testDate = DateTime.ParseExact(sortedBatchTestResults.Last().testResult.testDate, format, CultureInfo.InvariantCulture);

            List<BatchTestResult> timeCalculationTests = new List<BatchTestResult>();

            foreach (var result in sortedBatchTestResults)
            {
                DateTime checkTestDate = DateTime.ParseExact(result.testResult.testDate, format, CultureInfo.InvariantCulture);
                if (checkTestDate.Date == testDate.Date)
                {
                    timeCalculationTests.Add(result);
                }
            }

            for (int i = timeCalculationTests.Count - 1; i >= 0; i--)
            {
                if (i > 0)
                {
                    string endDateTimeStr = timeCalculationTests[i].testResult.testDate;
                    string startDateTimeStr = timeCalculationTests[i - 1].testResult.testDate;
                    DateTime endTestTime = DateTime.ParseExact(endDateTimeStr, format, CultureInfo.InvariantCulture);
                    DateTime startTestTime = DateTime.ParseExact(startDateTimeStr, format, CultureInfo.InvariantCulture);
                    TimeSpan timeSpan = endTestTime - startTestTime;
                    if (timeSpan < TimeSpan.FromMinutes(30))
                    {
                        totalRunTime += timeSpan;
                    }
                }
                else
                {
                    string endDateTimeStr = timeCalculationTests[i + 1].testResult.testDate;
                    string startDateTimeStr = timeCalculationTests[i].testResult.testDate;
                    DateTime endTestTime = DateTime.ParseExact(endDateTimeStr, format, CultureInfo.InvariantCulture);
                    DateTime startTestTime = DateTime.ParseExact(startDateTimeStr, format, CultureInfo.InvariantCulture);
                    TimeSpan timeSpan = endTestTime - startTestTime;
                    if (timeSpan < TimeSpan.FromMinutes(30))
                    {
                        totalRunTime += timeSpan;
                    }
                }
            }

            aveTestTimeTick = totalRunTime.Ticks / timeCalculationTests.Count();

            return aveTestTimeTick;
        }

        /// <summary>
        /// Saves or updates reports for the provided batch test results.
        /// Creates a new report entry, determines the file location, and for each batch test result,
        /// retrieves corresponding database batch test results and creates or updates their associated test result reports.
        /// Handles exceptions related to HTTP requests and general errors, displaying appropriate notifications and logging errors.
        /// </summary>
        /// <param name="batchTestResults">List of batch test results to be included in the report.</param>
        /// <param name="aveTimeTicks">The average test time in ticks to be recorded in the report.</param>
        /// <returns>True if the operation succeeds; otherwise, false.</returns>
        public async Task<bool> SaveOrUpdateReports(List<BatchTestResult> batchTestResults, long aveTimeTicks)
        {
            try
            {
                Report report = new Report
                {
                    createBy = Constants.Constants.Username,
                    createDate = DateTime.Now,
                    status = true,
                };

                var createdReport = await _reportRestAPI.CreateReportAsync(report);

                string fullFilePath = getFileLocation(batchTestResults);

                foreach (var batchTestResult in batchTestResults)
                {
                    List<BatchTestResult> dbBatchTestResults = await GetDbBatchTestResults(batchTestResult);
                    await CreateOrUpdateBatchTestResultReports(aveTimeTicks, createdReport, fullFilePath, batchTestResult, dbBatchTestResults);
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error while saving report. Please check internet connection and try again later.");
                LoggerUtility.LogError(ex);
                return false;
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error while saving report. Please try again later.");
                LoggerUtility.LogError(ex);
                return false;
            }
        }

        /// <summary>
        /// Creates or updates test result reports for each database batch test result.
        /// For each dbBatchTestResult, checks if a TestResultReport exists; if not, creates a new one with the provided data.
        /// If a report exists, updates it with the latest values from the batchTestResult and report context.
        /// </summary>
        /// <param name="aveTimeTicks">The average test time in ticks to be recorded in the report.</param>
        /// <param name="createdReport">The Report object representing the parent report entry.</param>
        /// <param name="fullFilePath">The file path where the report is stored.</param>
        /// <param name="batchTestResult">The BatchTestResult containing the latest test data.</param>
        /// <param name="dbBatchTestResults">List of BatchTestResult objects from the database to process.</param>
        private async Task CreateOrUpdateBatchTestResultReports(long aveTimeTicks, Report createdReport, string fullFilePath, BatchTestResult batchTestResult, List<BatchTestResult> dbBatchTestResults)
        {
            foreach (var dbBatchTestResult in dbBatchTestResults)
            {
                string testResultReportFilter = $"?batchTestResultId={dbBatchTestResult.id}";

                var batchTestResultReports = await _testResultReportRestAPI.GetAllTestResultReportAsync(filter: testResultReportFilter);

                if (batchTestResultReports == null || batchTestResultReports.Count == 0)
                {
                    TestResultReport firstTestResultReport = new TestResultReport
                    {
                        reportId = createdReport.id,
                        batchTestResultId = dbBatchTestResult.id,
                        standardReference = batchTestResult.standardReference,
                        torqueDiff = batchTestResult.torqueDiff,
                        fusionDiff = batchTestResult.fusionDiff,
                        result = batchTestResult.result,
                        aveTestTime = aveTimeTicks,
                        fileLocation = fullFilePath
                    };

                    await _testResultReportRestAPI.CreateTestResultReportAsync(firstTestResultReport);
                }
                else
                {
                    var existingBatchTestResultReport = batchTestResultReports.Last();

                    existingBatchTestResultReport.reportId = createdReport.id;
                    existingBatchTestResultReport.batchTestResultId = dbBatchTestResult.id;
                    existingBatchTestResultReport.standardReference = batchTestResult.standardReference;
                    existingBatchTestResultReport.torqueDiff = batchTestResult.torqueDiff;
                    existingBatchTestResultReport.fusionDiff = batchTestResult.fusionDiff;
                    existingBatchTestResultReport.result = batchTestResult.result;
                    existingBatchTestResultReport.aveTestTime = aveTimeTicks;
                    existingBatchTestResultReport.fileLocation = fullFilePath;

                    await _testResultReportRestAPI.UpdateTestResultReportAsync(existingBatchTestResultReport);
                }
            }
        }

        /// <summary>
        /// Retrieves the corresponding database batch test results for a given BatchTestResult.
        /// If the batch name contains a '+', it splits the batch into two and fetches results for both parts.
        /// Otherwise, it fetches results using the test result ID.
        /// </summary>
        /// <param name="batchTestResult">The BatchTestResult for which to retrieve database entries.</param>
        /// <returns>A list of BatchTestResult objects from the database matching the criteria.</returns>
        private async Task<List<BatchTestResult>> GetDbBatchTestResults(BatchTestResult batchTestResult)
        {
            var dbBatchTestResults = new List<BatchTestResult>();

            // check if batch contain + sign
            if (batchTestResult.batch.batchName.Contains("+"))
            {
                var productName = batchTestResult.testResult.product.name;
                var yearMonth = batchTestResult.batch.batchName.Substring(0, 3);
                var firstBatchName = batchTestResult.batch.batchName.Split('+')[0].Trim();
                var lastBatchName = $"{yearMonth}{batchTestResult.batch.batchName.Split('+')[1].Trim()}";
                var testNumber = batchTestResult.testResult.testNumber;

                string firstFilter = $"?productName={productName}&exactBatchName={firstBatchName}&testNumber={testNumber}";
                string secondFilter = $"?productName={productName}&exactBatchName={lastBatchName}&testNumber={testNumber}";

                var firstBatchTestResults = await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(filter: firstFilter);
                var secondBatchTestResults = await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(filter: secondFilter);

                dbBatchTestResults.AddRange(firstBatchTestResults);
                dbBatchTestResults.AddRange(secondBatchTestResults);
            }
            else
            {
                string testResultId = batchTestResult.testResult.id.ToString();
                string batchTestResultFilter = $"?testResultId={testResultId}";

                dbBatchTestResults = await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(filter: batchTestResultFilter);
            }

            return dbBatchTestResults;
        }

        /// <summary>
        /// Generates the file location for a report image based on the provided batch test results.
        /// Constructs the path using the report base folder, year, product name, month, and batch name(s).
        /// Handles both single and double batch scenarios, formatting the file name accordingly.
        /// Returns the full file path as a string.
        /// </summary>
        public string getFileLocation(List<BatchTestResult> batchTestResults)
        {
            string fullFilePath = "";

            List<BatchTestResult> bchBatchTestResults = batchTestResults.Where((bt) => bt.testResult.testType == "BCH").ToList();

            // file location
            string baseFolder = Constants.Constants.ReportFilePath;
            string yearFolder = "";
            string monthFolder = "";
            string productNameFolder = "";
            string fileName = "";

            if (bchBatchTestResults.Count != 0)
            {
                yearFolder = $"_20{bchBatchTestResults[0].batch.batchName.Substring(0, 2)}";
                monthFolder = bchBatchTestResults[0].batch.batchName.Substring(2, 1).ToUpper();
                productNameFolder = bchBatchTestResults[0].testResult.product.name.Replace("/", "-");

                if (bchBatchTestResults.Count == 1)
                {
                    fileName = $"{productNameFolder}_{bchBatchTestResults[0].batch.batchName}";
                }
                else
                {
                    string firstBatchName = bchBatchTestResults.FirstOrDefault().batch.batchName;
                    string lastBatchName = bchBatchTestResults.LastOrDefault().batch.batchName;

                    if (firstBatchName.Contains("+"))
                    {
                        firstBatchName = firstBatchName.Split('+')[0].Trim();
                    }

                    if (lastBatchName.Contains("+"))
                    {
                        string yearMonth = lastBatchName.Substring(0, 3);
                        lastBatchName = $"{yearMonth}{lastBatchName.Split('+')[1].Trim()}";
                    }

                    fileName = $"{productNameFolder}_{firstBatchName}-{lastBatchName}.png";
                }
            }

            fullFilePath = Path.Combine(baseFolder, yearFolder, productNameFolder, monthFolder, fileName);

            return fullFilePath;
        }

        /// <summary>
        /// Retrieves all batch test results for generating a report based on the specified product, batch range, test date, test number, and suffix.
        /// Filters the results by the provided suffix, consolidates double batches, loads standard and warm-up test results, 
        /// loads torque and fusion data, and orders the results by test date before returning the final list.
        /// Displays an error notification if no results are found for the given criteria.
        /// </summary>
        /// <param name="product">The product for which to retrieve batch test results.</param>
        /// <param name="fromBatch">The starting batch name in the range.</param>
        /// <param name="toBatch">The ending batch name in the range.</param>
        /// <param name="testDate">The test date to filter results.</param>
        /// <param name="testNumber">The test number to filter results.</param>
        /// <param name="suffix">The suffix to filter batch test results.</param>
        /// <returns>A list of BatchTestResult objects matching the specified criteria, ready for report generation.</returns>
        public async Task<List<BatchTestResult>> GetAllBatchTestResultsForMakingReport(Product product, string fromBatch, string toBatch, string testDate, string testNumber, string suffix)
        {
            var batchTestResults = await GetAllTestedResults(product, fromBatch, toBatch, testDate, testNumber);

            // fillter batch test results by suffix
            batchTestResults = batchTestResults.Where(r => r.batch.suffix == suffix).ToList();

            if (batchTestResults.Count == 0)
            {
                NotificationUtility.ShowError("No test results found for the given criteria.");
                return batchTestResults;
            }

            batchTestResults = ConsolidateDoubleBatchesTestResult(batchTestResults);

            batchTestResults = await LoadAllStandardTestResults(batchTestResults);

            batchTestResults = await LoadAllWarmUpTestResults(batchTestResults);

            batchTestResults = await LoadTorqueFusionData(batchTestResults);

            batchTestResults = OrderBatchTestResultsByTestDate(batchTestResults);


            return batchTestResults;
        }

        /// <summary>
        /// Orders the given list of BatchTestResult objects by their test date.
        /// Parses the test date from the testResult.testDate string property and assigns it to the testDate property of each BatchTestResult.
        /// Returns a new list sorted in ascending order by testDate.
        /// </summary>
        private List<BatchTestResult> OrderBatchTestResultsByTestDate(List<BatchTestResult> batchTestResults)
        {
            foreach (var batchTestResult in batchTestResults)
            {
                //batchTestResult.testDate = DateTime.ParseExact(batchTestResult.testResult.testDate, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                batchTestResult.testDate = DateTime.Parse(batchTestResult.testResult.testDate);
            }

            batchTestResults = batchTestResults.OrderBy(r => r.testDate).ToList();

            return batchTestResults;
        }

        /// <summary>
        /// Loads torque and fusion data for each BatchTestResult in the provided list.
        /// For each result, retrieves the latest Evaluation for torque (pointName "X") and fusion (pointName "t") from the database.
        /// Updates the testResult.torque, testResult.torqueId, testResult.fusion, and testResult.fusionId properties accordingly.
        /// Returns the updated list of BatchTestResult objects.
        /// </summary>
        private async Task<List<BatchTestResult>> LoadTorqueFusionData(List<BatchTestResult> batchTestResults)
        {
            foreach (var result in batchTestResults)
            {
                string xPointFilter = $"?testResultId={result.testResult.id}&pointName=X";
                string tPointFilter = $"?testResultId={result.testResult.id}&pointName=t";

                var evaluationX = await _evaluationRestAPI.GetAllEvaluationsAsync(filter: xPointFilter);
                var evaluationT = await _evaluationRestAPI.GetAllEvaluationsAsync(filter: tPointFilter);

                if (evaluationX != null && evaluationX.Count > 0)
                {
                    result.testResult.torque = Math.Round(evaluationX.Last().torque, 2);
                    result.testResult.torqueId = evaluationX.Last().id;
                }

                if (evaluationT != null && evaluationT.Count > 0)
                {
                    result.testResult.fusion = evaluationT.Last().timeEvalInt;
                    result.testResult.fusionId = evaluationT.Last().id;
                }
            }

            return batchTestResults;
        }

        /// <summary>
        /// Loads all warm-up (W/U) test results for the provided batch test results.
        /// For each unique test date in the input list, retrieves corresponding W/U batch test results
        /// from the database using product name, batch group, test date, and machine name as filters.
        /// Adds all found W/U results to the original list and returns the combined list.
        /// </summary>
        private async Task<List<BatchTestResult>> LoadAllWarmUpTestResults(List<BatchTestResult> batchTestResults)
        {
            List<DateTime> testDates = new List<DateTime>();

            List<BatchTestResult> wuBatchTestResults = new List<BatchTestResult>();

            string batchGroupName = batchTestResults[0].testResult.batchGroup;
            string machineName = batchTestResults[0].testResult.machine.name;
            string productName = batchTestResults[0].testResult.product.name;

            foreach (var batchTestResult in batchTestResults)
            {
                //var testDate = DateTime.ParseExact(batchTestResult.testResult.testDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).Date;
                var testDate = DateTime.Parse(batchTestResult.testResult.testDate).Date;

                if (!testDates.Contains(testDate))
                {
                    testDates.Add(testDate);
                }
            }

            // iterate through test dates and populate W/U test result
            foreach (var testDate in testDates)
            {
                string wuBatchResultsFilter = $"?productName={productName}&batchGroup={batchGroupName}&batchName=W/U&testDate={testDate}&machineName={machineName}";
                var results = await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(filter: wuBatchResultsFilter);

                if (results.Count > 0)
                {
                    wuBatchTestResults.AddRange(results);
                }
            }

            batchTestResults.AddRange(wuBatchTestResults);

            return batchTestResults;
        }

        /// <summary>
        /// Loads all standard (STD) test results for the provided batch test results.
        /// Retrieves STD batch test results from the database using product name, batch group, and machine name as filters.
        /// Adds all found STD results to the original list and returns the combined list.
        /// </summary>
        private async Task<List<BatchTestResult>> LoadAllStandardTestResults(List<BatchTestResult> batchTestResults)
        {
            string batchGroupName = batchTestResults[0].testResult.batchGroup;
            string machineName = batchTestResults[0].testResult.machine.name;
            string productName = batchTestResults[0].testResult.product.name;

            string stdResultsFilter = $"?productName={productName}&batchGroup={batchGroupName}&batchName=STD&machineName={machineName}";

            var stdBatchTestResults = await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(filter: stdResultsFilter);

            batchTestResults.AddRange(stdBatchTestResults);

            return batchTestResults;
        }

        /// <summary>
        /// Retrieves all tested batch results for a given product, batch range, test date, and attempt number.
        /// If a batch range is specified, fetches results for each batch in the range; otherwise, fetches by test date and attempt.
        /// Returns a list of BatchTestResult objects matching the criteria.
        /// </summary>
        private async Task<List<BatchTestResult>> GetAllTestedResults(Product product, string fromBatch, string toBatch, string testDate, string attempt)
        {
            var batchTestResults = new List<BatchTestResult>();

            // Get all batch test results for the given product name and batch name range
            if (!string.IsNullOrWhiteSpace(fromBatch))
            {
                var batches = !string.IsNullOrWhiteSpace(toBatch) ? BatchUtility.GenerateBatchNameFromTo(fromBatch, toBatch) : new List<string> { fromBatch };

                foreach (var batch in batches)
                {
                    var filter = $"?productName={product.name}&exactBatchName={batch}&testNumber={attempt}";
                    var batchTestResult = await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(filter);
                    batchTestResults.AddRange(batchTestResult);
                }

            }
            else
            {
                var filter = $"?productName={product.name}&testDate={testDate}&testNumber={attempt}";
                var batchTestResult = await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(filter);
                batchTestResults.AddRange(batchTestResult);
            }

            return batchTestResults;
        }

        /// <summary>
        /// Consolidates double batch test results by combining consecutive results with the same file name.
        /// Sets the batch number for ordering, sorts the list, and merges batch names for double batches.
        /// Removes the second batch from the list when a double batch is detected.
        /// Returns the consolidated and ordered list of BatchTestResult objects.
        /// </summary>
        private List<BatchTestResult> ConsolidateDoubleBatchesTestResult(List<BatchTestResult> batchTestResults)
        {
            try
            {
                // substring batch name to get batch number in int for ordering
                foreach (var batchTestResult in batchTestResults)
                {
                    batchTestResult.batch.batchNumber = int.Parse(batchTestResult.batch.batchName.Substring(3));
                }
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Batch number format is not correct.");
                LoggerUtility.LogError(ex);
            }

            // order batchTestResults by batch name
            batchTestResults = batchTestResults.OrderBy(r => r.batch.batchNumber).ToList();

            for (int i = 0; i < batchTestResults.Count - 1; i++)
            {
                if (batchTestResults[i] != null)
                {
                    if (batchTestResults[i].testResult.fileName == batchTestResults[i + 1].testResult.fileName)
                    {
                        batchTestResults[i].batch.batchName = $"{batchTestResults[i].batch.batchName}+{batchTestResults[i + 1].batch.batchName.Substring(3)}";
                        batchTestResults.RemoveAt(i + 1);
                    }
                }
            }

            return batchTestResults;
        }

        /// <summary>
        /// Calculates the torque and fusion differences for each batch test result compared to a standard result.
        /// For each batch test result of type "BCH", sets the standard reference, computes the percentage difference
        /// in torque and fusion values relative to the standard, and determines if the result passes based on product fail thresholds.
        /// </summary>
        /// <param name="batchTestResults">List of BatchTestResult objects to update with calculated differences and results.</param>
        /// <param name="standardResult">The BatchTestResult representing the standard for comparison.</param>
        /// <returns>The updated list of BatchTestResult objects with calculated differences and result flags.</returns>
        public List<BatchTestResult> CalculateTorDiffAndFusDiff(List<BatchTestResult> batchTestResults, BatchTestResult standardResult)
        {
            if (standardResult != null)
            {
                var refTorque = standardResult.testResult.torque;
                var refFusion = (double)standardResult.testResult.fusion;
                var selectedSTD = standardResult.batch.batchName;

                var torqueFail = standardResult.testResult.product.torqueFail;
                var fusionFail = standardResult.testResult.product.fusionFail;

                foreach (var batchTestResult in batchTestResults)
                {
                    if (batchTestResult.testResult.testType == "BCH")
                    {
                        batchTestResult.standardReference = selectedSTD;

                        batchTestResult.torqueDiff = Math.Round((batchTestResult.testResult.torque - refTorque) / refTorque * 100, 2);

                        batchTestResult.fusionDiff = Math.Round((batchTestResult.testResult.fusion - refFusion) / refFusion * 100, 2);

                        batchTestResult.result = Math.Abs(batchTestResult.torqueDiff) <= torqueFail && Math.Abs(batchTestResult.fusionDiff) <= fusionFail;
                    }
                }
            }

            return batchTestResults;
        }

        /// <summary>
        /// Checks and updates the torque and fusion evaluation results for each batch test result.
        /// For each batch test result, retrieves the corresponding torque and fusion evaluation records from the database.
        /// If the torque or fusion values differ from those in the evaluation records, updates the evaluation records accordingly.
        /// Displays a success notification listing the batches for which torque or fusion data was updated.
        /// Returns the updated list of batch test results.
        /// </summary>
        public async Task<List<BatchTestResult>> CheckAndUpdateEvaluationResults(List<BatchTestResult> batchTestResults)
        {
            var updatedBatchTestResults = new List<BatchTestResult>();

            List<string> updatedTorqueBatchName = new List<string>();
            List<string> updatedFusionBatchName = new List<string>();

            if (batchTestResults.Count > 0)
            {
                foreach (var batchTestResult in batchTestResults)
                {
                    var torqueEvaluationId = batchTestResult.testResult.torqueId;
                    var fusionEvaluationId = batchTestResult.testResult.fusionId;

                    // load evaluation data from database
                    var torqueEvaluation = await _evaluationRestAPI.GetEvaluationByIdAsync(torqueEvaluationId);
                    var fusionEvaluation = await _evaluationRestAPI.GetEvaluationByIdAsync(fusionEvaluationId);


                    // check if difference from result in batch test result is different from evaluation data
                    if (Math.Round(torqueEvaluation.torque, 2) != batchTestResult.testResult.torque)
                    {
                        torqueEvaluation.torque = batchTestResult.testResult.torque;
                        torqueEvaluation.testResultId = torqueEvaluation.testResult.id;
                        await _evaluationRestAPI.UpdateEvaluationAsync(torqueEvaluation);

                        updatedTorqueBatchName.Add(batchTestResult.batch.batchName);
                    }

                    if (fusionEvaluation.timeEvalInt != batchTestResult.testResult.fusion)
                    {
                        fusionEvaluation.timeEvalInt = batchTestResult.testResult.fusion;
                        fusionEvaluation.testResultId = fusionEvaluation.testResult.id;
                        await _evaluationRestAPI.UpdateEvaluationAsync(fusionEvaluation);

                        updatedFusionBatchName.Add(batchTestResult.batch.batchName);
                    }

                    updatedBatchTestResults.Add(batchTestResult);
                }

                if (updatedTorqueBatchName.Count > 0 || updatedFusionBatchName.Count > 0)
                {
                    string torqueUpdatedMessage = updatedTorqueBatchName.Count > 0 ? $"Torque data updated for batches: {string.Join(", ", updatedTorqueBatchName)}" : "";
                    string fusionUpdatedMessage = updatedFusionBatchName.Count > 0 ? $"Fusion data updated for batches: {string.Join(", ", updatedFusionBatchName)}" : "";

                    NotificationUtility.ShowSuccess($"{torqueUpdatedMessage}\n{fusionUpdatedMessage}");
                }
            }


            return updatedBatchTestResults;
        }

        /// <summary>
        /// Deletes a specified TestResultReport after confirming the delete action.
        /// If the delete confirmation is invalid, returns null. Otherwise, deletes the report using the REST API.
        /// </summary>
        /// <param name="testResultReport">The TestResultReport object to delete.</param>
        /// <param name="deleteConfirmation">The confirmation string to validate the delete action.</param>
        /// <returns>The deleted TestResultReport object if successful; otherwise, null.</returns>
        public async Task<TestResultReport> DeleteTestResultReport(TestResultReport testResultReport, string deleteConfirmation)
        {
            if (!InputValidationUtility.DeleteConfirmation(deleteConfirmation)) return null;

            return await _testResultReportRestAPI.DeleteTestResultReportAsync(testResultReport);
        }

        /// <summary>
        /// Retrieves all TestResultReport objects for a given product and batch range, filtered by test number and suffix.
        /// Handles single batch, exact batch, and batch range scenarios. Returns only reports matching the specified suffix.
        /// </summary>
        /// <param name="selectedProduct">The product for which to retrieve reports.</param>
        /// <param name="fromBatch">The starting batch name in the range.</param>
        /// <param name="toBatch">The ending batch name in the range.</param>
        /// <param name="testNumber">The test number to filter reports.</param>
        /// <param name="suffix">The suffix to filter reports.</param>
        /// <returns>A list of TestResultReport objects matching the criteria.</returns>
        public async Task<List<TestResultReport>> GetProductTestResultReportsWithBatchRange(Product selectedProduct, string fromBatch, string toBatch, string testNumber, string suffix)
        {
            List<TestResultReport> reports = new List<TestResultReport>();

            try
            {
                if (fromBatch.Length == 3)
                {
                    reports = await _testResultReportRestAPI.GetAllTestResultReportAsync($"?productName={selectedProduct.name}&batchName={fromBatch}&testNumber={testNumber}");
                }
                else if (fromBatch.Length > 3 && (fromBatch == toBatch || string.IsNullOrWhiteSpace(toBatch)))
                {
                    reports = await _testResultReportRestAPI.GetAllTestResultReportAsync($"?productName={selectedProduct.name}&exactBatchName={fromBatch}&testNumber={testNumber}");
                }
                else
                {
                    var batches = BatchUtility.GenerateBatchNameFromTo(fromBatch, toBatch);
                    if (batches.Count == 0)
                    {
                        return reports;
                    }

                    foreach (string batchName in batches)
                    {
                        string filters = $"?productName={selectedProduct.name}&exactBatchName={batchName}&testNumber={testNumber}";
                        var batchReports = await _testResultReportRestAPI.GetAllTestResultReportAsync(filters);
                        reports.AddRange(batchReports);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError(ex);
            }

            // filter reports by suffix
            reports = reports.Where(r => r.batchTestResult.batch.suffix == suffix).ToList();

            return reports;
        }
    }
}
