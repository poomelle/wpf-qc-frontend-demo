using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.DataLoaderService
{
    public interface ISearchDataLoaderService
    {
        Task<List<BatchTestResult>> LoadBCHBatchTestResult(string productName, string testNumber, string suffix, string fromBatchName, string toBatchName, DateTime testDate);
        Task<List<BatchTestResult>> LoadWarmUpAndSTDBatchTestResult(List<BatchTestResult> bchBatchTestResults, string testType);
    }
}
