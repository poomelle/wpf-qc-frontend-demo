using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI.IRestAPI
{
    public interface IBatchRestAPI
    {
        Task<List<Batch>> GetAllBatchesAsync(string filter = "", string sort = "");
        Task<Batch> GetBatchByIdAsync(int id);
        Task<Batch> CreateBatchAsync(Batch batch);
        Task<Batch> UpdateBatchAsync(Batch batch);
        Task<Batch> DeleteBatchAsync(Batch batch);
    }
}
