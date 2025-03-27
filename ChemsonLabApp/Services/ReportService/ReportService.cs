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

        public async Task<List<BatchTestResult>> GetAllBatchTestResultsForMakingReport(Product product, string fromBatch, string toBatch, string testDate, string attempt)
        {
            var batchTestResults = await GetAllTestedResults(product, fromBatch, toBatch, testDate, attempt);

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

        private List<BatchTestResult> OrderBatchTestResultsByTestDate(List<BatchTestResult> batchTestResults)
        {
            foreach (var batchTestResult in batchTestResults)
            {
                batchTestResult.testDate = DateTime.ParseExact(batchTestResult.testResult.testDate, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }

            batchTestResults = batchTestResults.OrderBy(r => r.testDate).ToList();

            return batchTestResults;
        }

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

        private async Task<List<BatchTestResult>> LoadAllWarmUpTestResults(List<BatchTestResult> batchTestResults)
        {
            List<DateTime> testDates = new List<DateTime>();

            List<BatchTestResult> wuBatchTestResults = new List<BatchTestResult>();

            string batchGroupName = batchTestResults[0].testResult.batchGroup;
            string machineName = batchTestResults[0].testResult.machine.name;
            string productName = batchTestResults[0].testResult.product.name;

            foreach (var batchTestResult in batchTestResults)
            {
                var testDate = DateTime.ParseExact(batchTestResult.testResult.testDate, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture).Date;

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

        public async Task<TestResultReport> DeleteTestResultReport(TestResultReport testResultReport, string deleteConfirmation)
        {
            if (!InputValidationUtility.DeleteConfirmation(deleteConfirmation)) return null;

            return await _testResultReportRestAPI.DeleteTestResultReportAsync(testResultReport);
        }

        public async Task<List<TestResultReport>> GetProductTestResultReportsWithBatchRange(Product selectedProduct, string fromBatch, string toBatch)
        {
            List<TestResultReport> reports = new List<TestResultReport>();

            try
            {
                if (fromBatch.Length == 3)
                {
                    reports = await _testResultReportRestAPI.GetAllTestResultReportAsync($"?productName={selectedProduct.name}&batchName={fromBatch}");
                }
                else if (fromBatch.Length > 3 && (fromBatch == toBatch || string.IsNullOrWhiteSpace(toBatch)))
                {
                    reports = await _testResultReportRestAPI.GetAllTestResultReportAsync($"?productName={selectedProduct.name}&exactBatchName={fromBatch}");
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
                        string filters = $"?productName={selectedProduct.name}&exactBatchName={batchName}";
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

            return reports;
        }
    }
}
