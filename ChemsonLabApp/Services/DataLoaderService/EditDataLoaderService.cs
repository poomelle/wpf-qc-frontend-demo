﻿using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChemsonLabApp.Services.DataLoaderService
{
    public class EditDataLoaderService : IEditDataLoaderService
    {
        private readonly IBatchRestAPI _batchRestAPI;
        private readonly ITestResultRestAPI _testResultRestAPI;
        private readonly IEvaluationRestAPI _evaluationRestAPI;
        private readonly IBatchTestResultRestAPI _batchTestResultRestAPI;

        public EditDataLoaderService(
            IBatchRestAPI batchRestAPI,
            ITestResultRestAPI testResultRestAPI,
            IEvaluationRestAPI evaluationRestAPI,
            IBatchTestResultRestAPI batchTestResultRestAPI)
        {
            this._batchRestAPI = batchRestAPI;
            this._testResultRestAPI = testResultRestAPI;
            this._evaluationRestAPI = evaluationRestAPI;
            this._batchTestResultRestAPI = batchTestResultRestAPI;
        }

        /// <summary>
        /// Retrieves batch information for the specified BatchTestResult.
        /// </summary>
        /// <param name="batchTestResult">The BatchTestResult containing the batch reference.</param>
        /// <returns>The Batch associated with the BatchTestResult.</returns>
        public async Task<Batch> GetBatchInformation(BatchTestResult batchTestResult)
        {
            return await _batchRestAPI.GetBatchByIdAsync(batchTestResult.batch.id);
        }

        /// <summary>
        /// Retrieves test result information for the specified BatchTestResult.
        /// </summary>
        /// <param name="batchTestResult">The BatchTestResult containing the test result reference.</param>
        /// <returns>The TestResult associated with the BatchTestResult.</returns>
        public async Task<TestResult> GetTestResultInformation(BatchTestResult batchTestResult)
        {
            return await _testResultRestAPI.GetTestResultByIdAsync(batchTestResult.testResult.id);
        }

        /// <summary>
        /// Retrieves the Evaluation at a specific point for the given BatchTestResult.
        /// </summary>
        /// <param name="batchTestResult">The BatchTestResult containing the test result reference.</param>
        /// <param name="point">The point name to filter evaluations.</param>
        /// <returns>The Evaluation at the specified point.</returns>
        public async Task<Evaluation> GetEvaluationAtPoint(BatchTestResult batchTestResult, string point)
        {
            string pointFilter = $"?testResultId={batchTestResult.testResult.id}&pointName={point}";

            var evaluations = await _evaluationRestAPI.GetAllEvaluationsAsync(filter: pointFilter);

            return evaluations.Last();
        }

        /// <summary>
        /// Updates the data loader with the provided test result, batch, batch test result, and evaluations.
        /// Ensures uniqueness by product name, batch name, and test number before updating.
        /// </summary>
        /// <param name="testResult">The TestResult to update.</param>
        /// <param name="batch">The Batch to update.</param>
        /// <param name="batchTestResult">The BatchTestResult for uniqueness checking.</param>
        /// <param name="evaluationX">The Evaluation at point X to update.</param>
        /// <param name="evaluationT">The Evaluation at point T to update.</param>
        public async Task UpdateDataLoader(TestResult testResult, Batch batch, BatchTestResult batchTestResult, Evaluation evaluationX, Evaluation evaluationT)
        {
            // Checking Uniqueness by product name, exact batch name and testNumber
            var productName = testResult.product.name;
            var batchName = batch.batchName;
            var testNumber = testResult.testNumber;
            var filter = $"?productName={productName}&batchName={batchName}&testNumber={testNumber}";

            var batchTestResults = await _batchTestResultRestAPI.GetAllBatchTestResultsAsync(filter: filter);

            if (batchTestResults.Count() == 1 && batchTestResult.id != batchTestResults.Last().id)
            {
                MessageBox.Show($"The Test result has existed", "Update Fail!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else if (batchTestResults.Count == 0 || (batchTestResults.Count() == 1 && batchTestResult.id == batchTestResults.Last().id))
            {
                testResult.productId = testResult.product.id;
                testResult.machineId = testResult.machine.id;
                await _testResultRestAPI.UpdateTestResultAsync(testResult);

                // Update Evaluation X point
                evaluationX.testResultId = evaluationX.testResult.id;
                await _evaluationRestAPI.UpdateEvaluationAsync(evaluationX);

                // Update Evaluation t point
                evaluationT.testResultId = evaluationT.testResult.id;
                await _evaluationRestAPI.UpdateEvaluationAsync(evaluationT);

                // Update Batch
                batch.productId = batch.product.id;
                await _batchRestAPI.UpdateBatchAsync(batch);
            }
        }
    }
}
