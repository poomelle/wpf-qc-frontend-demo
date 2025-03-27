using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI.IRestAPI
{
    public interface IQcPerformanceKpiRestAPI
    {
        Task<List<QcPerformanceKpi>> GetAllQcPerformanceKpisAsync(string filter = "", string sort = "");
    }
}
