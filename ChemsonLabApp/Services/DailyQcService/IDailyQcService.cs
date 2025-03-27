using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.IService
{
    public interface IDailyQcService
    {
        Task<List<DailyQc>> GetAllDailyQcsAsync(string filter = "", string sort = "");
        Task<DailyQc> GetDailyQcByIdAsync(int id);
        Task<DailyQc> CreateDailyQcAsync(DailyQc dailyQc);
        Task<DailyQc> UpdateDailyQcAsync(DailyQc dailyQc);
        Task<bool> DeleteDailyQcAsync(List<DailyQc> dailyQcs);
        Task<List<DailyQc>> LoadTodayDashboardDailyQcAsync(string productName, string year, string month, string status);
        Task<string> GetLastCoaBatchName(DailyQc dailyQc);
        Task<string> GetLastTest(DailyQc dailyQc);
        Task<string> GetDailyQcLabel(DailyQc dailyQc);
        int GetMixReqd(DailyQc dailyQc);
        Task<List<DailyQc>> GetDailyQcs(string productName, string year, string month, string status);
        Task SaveAllDailyQcs(List<DailyQc> dailyQcs);
        Task<List<DailyQc>> PopulateCOALabelBatchMixReqd(List<DailyQc> dailyQcs);
    }
}
