using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services
{
    public class CoaService : ICoaService
    {
        private readonly ICoaRestAPI _coaRestAPI;
        private readonly ITestResultReportRestAPI _testResultReportRestAPI;

        public CoaService(ICoaRestAPI coaRestAPI, ITestResultReportRestAPI testResultReportRestAPI)
        {
            this._coaRestAPI = coaRestAPI;
            this._testResultReportRestAPI = testResultReportRestAPI;
        }

        public async Task CreateCOAFromTestResultReportAsync(List<TestResultReport> testResultReports)
        {
            if (testResultReports != null || testResultReports.Count != 0)
            {
                foreach (var testResultReport in testResultReports)
                {
                    try
                    {
                        string filter = $"?productName={testResultReport.batchTestResult.testResult.product.name}&batchName={testResultReport.batchTestResult.batch.batchName}";
                        var coas = await _coaRestAPI.GetAllCoasAsync(filter);

                        if (coas.Count == 0)
                        {
                            Coa coa = new Coa
                            {
                                productId = testResultReport.batchTestResult.testResult.product.id,
                                batchName = testResultReport.batchTestResult.batch.batchName
                            };

                            await _coaRestAPI.CreateCoaAsync(coa);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error and continue
                        LoggerUtility.LogError(ex);
                        continue;
                    }
                }
            }
        }

        public async Task<List<TestResultReport>> GetProductTestResultReportsWithBatchRange(Product selectedProduct, string fromBatch, string toBatch)
        {
            List<TestResultReport> reports = new List<TestResultReport>();

            try
            {
                if (fromBatch.Length == 3)
                {
                    reports = await _testResultReportRestAPI.GetAllTestResultReportAsync($"?productName={selectedProduct.name}&batchName={fromBatch}");
                }
                else if (fromBatch.Length > 3 && (fromBatch == toBatch || string.IsNullOrWhiteSpace(toBatch)))
                {
                    reports = await _testResultReportRestAPI.GetAllTestResultReportAsync($"?productName={selectedProduct.name}&exactBatchName={fromBatch}");
                }
                else
                {
                    var batches = BatchUtility.GenerateBatchNameFromTo(fromBatch, toBatch);
                    if (batches.Count == 0)
                    {
                        return reports;
                    }

                    foreach (string batchName in batches)
                    {
                        string filters = $"?productName={selectedProduct.name}&exactBatchName={batchName}";
                        var batchReports = await _testResultReportRestAPI.GetAllTestResultReportAsync(filters);
                        reports.AddRange(batchReports);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError(ex);
            }

            return reports;
        }

        public List<TestResultReport> SortTestReportsByBatchNumber(List<TestResultReport> testResultReports)
        {
            foreach (var report in testResultReports)
            {
                if (int.TryParse(report.batchTestResult.batch.batchName.Substring(3), out int batchNumber))
                {
                    report.batchNumber = batchNumber;
                }
            }
            return testResultReports.OrderBy(report => report.batchNumber).ToList();
        }
    }
}
