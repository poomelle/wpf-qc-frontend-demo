using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.DataLoaderService
{
    public interface IDeleteDataLoaderService
    {
        Task DeleteBatchTestResults(List<BatchTestResult> batchTestResults, string deleteConfirm);
    }
}
