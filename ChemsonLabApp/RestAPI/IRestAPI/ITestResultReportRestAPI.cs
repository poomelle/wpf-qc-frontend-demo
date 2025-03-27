using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI.IRestAPI
{
    public interface ITestResultReportRestAPI
    {
        Task<List<TestResultReport>> GetAllTestResultReportAsync(string filter = "", string sort = "");
        Task<TestResultReport> GetTestResultReportByIdAsync(int id);
        Task<TestResultReport> CreateTestResultReportAsync(TestResultReport testResultReport);
        Task<TestResultReport> UpdateTestResultReportAsync(TestResultReport testResultReport);
        Task<TestResultReport> DeleteTestResultReportAsync(TestResultReport testResultReport);
    }
}
