using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.COAService
{
    public interface ICoaService
    {
        Task CreateCOAFromTestResultReportAsync(List<TestResultReport> testResultReports);
        Task<List<TestResultReport>> GetProductTestResultReportsWithBatchRange(Product product, string fromBatch, string toBatch);
        List<TestResultReport> SortTestReportsByBatchNumber(List<TestResultReport> testResultReports);

    }
}
