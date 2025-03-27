using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI.IRestAPI
{
    public interface IReportRestAPI
    {
        Task<List<Report>> GetAllReportAsync(string filter = "", string sort = "");
        Task<Report> GetReportByIdAsync(int id);
        Task<Report> CreateReportAsync(Report report);
        Task<Report> UpdateReportAsync(Report report);
        Task<Report> DeleteReportAsync(Report report);
    }
}
