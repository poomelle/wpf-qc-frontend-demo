using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.DataLoaderService
{
    public interface IEditDataLoaderService
    {
        Task<Batch> GetBatchInformation(BatchTestResult batchTestResult);
        Task<TestResult> GetTestResultInformation(BatchTestResult batchTestResult);
        Task<Evaluation> GetEvaluationAtPoint(BatchTestResult batchTestResult, string point);
        Task UpdateDataLoader(TestResult testResult, Batch batch, BatchTestResult batchTestResult, Evaluation evaluationX, Evaluation evaluationT);
    }
}
