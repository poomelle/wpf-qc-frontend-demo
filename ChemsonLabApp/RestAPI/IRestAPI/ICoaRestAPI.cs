using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI.IRestAPI
{
    public interface ICoaRestAPI
    {
        Task<List<Coa>> GetAllCoasAsync(string filter = "", string sort = "");
        Task<Coa> GetCoaByIdAsync(int id);
        Task<Coa> CreateCoaAsync(Coa coa);
        Task<Coa> UpdateCoaAsync(Coa coa);
        Task<Coa> DeleteCoaAsync(Coa coa);
    }
}
