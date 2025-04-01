using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.IService
{
    public interface IReportService
    {
        Task<bool> SaveOrUpdateReports(List<BatchTestResult> batchTestResults, long aveTimeTicks);
        long CalculateAveTestTimeTick(List<BatchTestResult> batchTestResults);
        string getFileLocation(List<BatchTestResult> batchTestResults);
        Task<List<BatchTestResult>> GetAllBatchTestResultsForMakingReport(Product product, string fromBatch, string toBatch, string testDate, string testNumber, string suffix);
        List<BatchTestResult> CalculateTorDiffAndFusDiff(List<BatchTestResult> batchTestResults, BatchTestResult standardResult);
        Task<List<BatchTestResult>> CheckAndUpdateEvaluationResults(List<BatchTestResult> batchTestResults);
        Task<TestResultReport> DeleteTestResultReport(TestResultReport testResultReport, string deleteConfirmation);
        Task<List<TestResultReport>> GetProductTestResultReportsWithBatchRange(Product product, string fromBatch, string toBatch, string testNumber, string suffix);
    }
}
