using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI.IRestAPI
{
    public interface IInstrumentRestAPI
    {
        Task<List<Instrument>> GetInstrumentsAsync(string filter = "", string sort = "");
        Task<Instrument> GetInstrumentByIdAsync(int id);
        Task<Instrument> CreateInstrumentAsync(Instrument instrument);
        Task<Instrument> UpdateInstrumentAsync(Instrument instrument);
        Task<Instrument> DeleteInstrumentAsync(Instrument instrument);
    }
}
