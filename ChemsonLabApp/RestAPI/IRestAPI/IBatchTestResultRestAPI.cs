using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI.IRestAPI
{
    public interface IBatchTestResultRestAPI
    {
        Task<List<BatchTestResult>> GetAllBatchTestResultsAsync(string filter = "", string sort = "");
        Task<BatchTestResult> GetBatchTestResultById(int id);
        Task<BatchTestResult> CreateBatchTestResultAsync(BatchTestResult batchTestResult);
        Task<BatchTestResult> UpdateBatchTestResultAsync(BatchTestResult batchTestResult);
        Task<BatchTestResult> DeleteBatchTestResultAsync(BatchTestResult batchTestResult);
    }
}
