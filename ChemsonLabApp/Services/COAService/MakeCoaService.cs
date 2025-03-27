﻿using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.COAService
{
    public class MakeCoaService : IMakeCoaService
    {
        private readonly ICustomerOrderRestAPI _customerOrderRestAPI;
        private readonly ICoaRestAPI _coaRestAPI;

        public MakeCoaService(ICustomerOrderRestAPI customerOrderRestAPI, ICoaRestAPI coaRestAPI)
        {
            this._customerOrderRestAPI = customerOrderRestAPI;
            this._coaRestAPI = coaRestAPI;
        }

        public async Task<List<CustomerOrder>> GetAllCustomerOrdersAsync(string filter = "", string sort = "")
        {
            return await _customerOrderRestAPI.GetAllCustomerOrdersAsync(filter, sort);
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
    }
}
