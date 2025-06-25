using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.DataLoaderService
{
    public class DeleteDataLoaderService : IDeleteDataLoaderService
    {
        private readonly ITestResultReportRestAPI _testResultReportRestAPI;
        private readonly IBatchTestResultRestAPI _batchTestResultRestAPI;
        private readonly IBatchRestAPI _batchRestAPI;
        private readonly IEvaluationRestAPI _evaluationRestAPI;
        private readonly IMeasurementRestAPI _measurementRestAPI;
        private readonly ITestResultRestAPI _testResultRestAPI;

        public DeleteDataLoaderService(
            ITestResultReportRestAPI testResultReportRestAPI,
            IBatchTestResultRestAPI batchTestResultRestAPI,
            IBatchRestAPI batchRestAPI,
            IEvaluationRestAPI evaluationRestAPI,
            IMeasurementRestAPI measurementRestAPI,
            ITestResultRestAPI testResultRestAPI)
        {
            this._testResultReportRestAPI = testResultReportRestAPI;
            this._batchTestResultRestAPI = batchTestResultRestAPI;
            this._batchRestAPI = batchRestAPI;
            this._evaluationRestAPI = evaluationRestAPI;
            this._measurementRestAPI = measurementRestAPI;
            this._testResultRestAPI = testResultRestAPI;
        }


        /// <summary>
        /// Deletes a list of BatchTestResult entities and all related data (TestResultReports, Batch, Evaluation, Measurement, TestResult)
        /// after confirming the delete operation.
        /// </summary>
        /// <param name="batchTestResults">The list of BatchTestResult objects to delete.</param>
        /// <param name="deleteConfirm">The confirmation string required to proceed with deletion.</param>
        public async Task DeleteBatchTestResults(List<BatchTestResult> batchTestResults, string deleteConfirm)
        {
            if (!InputValidationUtility.DeleteConfirmation(deleteConfirm)) return;

            foreach (var batchTestResult in batchTestResults)
            {
                await DeleteBatchTestResult(batchTestResult);
            }
        }

        /// <summary>
        /// Deletes a single BatchTestResult and all associated entities, including TestResultReports, Batch, Evaluations, Measurements, and TestResult.
        /// </summary>
        /// <param name="batchTestResult">The BatchTestResult object to delete.</param>
        private async Task DeleteBatchTestResult(BatchTestResult batchTestResult)
        {
            var batchTestResultId = batchTestResult.id;
            var testResultId = batchTestResult.testResult.id;

            // Delete TestResultReport if any
            string batchTestResultReportFilter = $"?batchTestResultId={batchTestResultId}";
            var batchTestResultReports = await _testResultReportRestAPI.GetAllTestResultReportAsync(filter: batchTestResultReportFilter);

            if (batchTestResultReports.Count > 0)
            {
                foreach (var batchTestResultReport in batchTestResultReports)
                {
                    await _testResultReportRestAPI.DeleteTestResultReportAsync(batchTestResultReport);
                }
            }

            // Delete BatchTestResult
            await _batchTestResultRestAPI.DeleteBatchTestResultAsync(batchTestResult);

            // Delete Batch
            var deleteBatch = batchTestResult.batch;
            await _batchRestAPI.DeleteBatchAsync(deleteBatch);

            // Delete Evaluation
            string evaluationFilter = $"?testResultId={testResultId}";
            var evaluations = await _evaluationRestAPI.GetAllEvaluationsAsync(filter: evaluationFilter);
            if (evaluations.Count > 0)
            {
                foreach (var evaluation in evaluations)
                {
                    await _evaluationRestAPI.DeleteEvaluationAsync(evaluation);
                }
            }

            // Delete Measurement
            string measurementFilter = $"?testResultId={testResultId}";
            var measurements = await _measurementRestAPI.GetAllMeasurementsAsync(filter: measurementFilter);
            if (measurements.Count > 0)
            {
                foreach (var measurement in measurements)
                {
                    await _measurementRestAPI.DeleteMeasurementAsync(measurement);
                }
            }

            // Delete TestResult
            var deleteTestResult = batchTestResult.testResult;
            await _testResultRestAPI.DeleteTestResultAsync(deleteTestResult);
        }
    }
}
