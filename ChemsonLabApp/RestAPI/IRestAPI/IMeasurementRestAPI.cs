using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI.IRestAPI
{
    public interface IMeasurementRestAPI
    {
        Task<List<Measurement>> GetAllMeasurementsAsync(string filter = "", string sort = "");
        Task<Measurement> GetMeasurementByIdAsync(int id);
        Task<Measurement> CreateMeasurementAsync(Measurement measurement);
        Task<Measurement> UpdateMeasurementAsync(Measurement measurement);
        Task<Measurement> DeleteMeasurementAsync(Measurement measurement);
    }
}
