using ChemsonLabApp.MVVM.Models;
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

        public async Task<Batch> GetBatchInformation(BatchTestResult batchTestResult)
        {
            return await _batchRestAPI.GetBatchByIdAsync(batchTestResult.batch.id);
        }

        public async Task<TestResult> GetTestResultInformation(BatchTestResult batchTestResult)
        {
            return await _testResultRestAPI.GetTestResultByIdAsync(batchTestResult.testResult.id);
        }

        public async Task<Evaluation> GetEvaluationAtPoint(BatchTestResult batchTestResult, string point)
        {
            string pointFilter = $"?testResultId={batchTestResult.testResult.id}&pointName={point}";

            var evaluations = await _evaluationRestAPI.GetAllEvaluationsAsync(filter: pointFilter);

            return evaluations.Last();
        }

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
