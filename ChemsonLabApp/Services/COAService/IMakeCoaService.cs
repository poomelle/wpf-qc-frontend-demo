using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.COAService
{
    public interface IMakeCoaService
    {
        Task<List<CustomerOrder>> GetAllCustomerOrdersAsync(string filter = "", string sort = "");
        Task CreateCOAFromTestResultReportAsync(List<TestResultReport> testResultReports);
    }
}
