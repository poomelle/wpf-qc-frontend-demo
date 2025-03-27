using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI.IRestAPI
{
    public interface ITestResultRestAPI
    {
        Task<List<TestResult>> GetAllTestResultsAsync(string filter = "", string sort = "");
        Task<TestResult> GetTestResultByIdAsync(int id);
        Task<TestResult> CreateTestResultAsync(TestResult testResult);
        Task<TestResult> UpdateTestResultAsync(TestResult testResult);
        Task<TestResult> DeleteTestResultAsync(TestResult testResult);
    }
}
