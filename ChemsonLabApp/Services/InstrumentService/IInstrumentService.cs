using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.IService
{
    public interface IInstrumentService
    {
        Task<List<Instrument>> GetAllInstrumentsAsync(string filter = "", string sort = "");
        Task<bool> CreateInstrumentAsync(string name);
        Task<bool> UpdateInstrumentAsync(Instrument instrument);
        Task<Instrument> DeleteInstrumentAsync(Instrument instrument, string deleteConfirmation);
        Task<List<Instrument>> GetAllActiveInstrument();
        Task<List<string>> GetAllActiveInstrumentName();
    }
}
