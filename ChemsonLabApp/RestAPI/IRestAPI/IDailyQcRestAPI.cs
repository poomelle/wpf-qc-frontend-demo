using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI.IRestAPI
{
    public interface IDailyQcRestAPI
    {
        Task<List<DailyQc>> GetAllDailyQcsAsync(string filter = "", string sort = "");
        Task<DailyQc> GetDailyQcByIdAsync(int id);
        Task<DailyQc> CreateDailyQcAsync(DailyQc dailyQc);
        Task<DailyQc> UpdateDailyQcAsync(DailyQc dailyQc);
        Task<DailyQc> DeleteDailyQcAsync(DailyQc dailyQc);
    }
}
