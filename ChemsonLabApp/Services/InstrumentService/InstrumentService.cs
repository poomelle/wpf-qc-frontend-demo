using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services
{
    public class InstrumentService : IInstrumentService
    {
        private readonly IInstrumentRestAPI _instrumentRestAPI;

        public InstrumentService(IInstrumentRestAPI instrumentRestAPI)
        {
            this._instrumentRestAPI = instrumentRestAPI;
        }

        public async Task<bool> CreateInstrumentAsync(string name)
        {
            var isNewInstrumentNameValid = await IsAddNewInstrumentNameValid(name);

            if (!isNewInstrumentNameValid)
            {
                NotificationUtility.ShowError("Instrument name is invalid or already exists");
                return false;
            }

            var instrument = new Instrument
            {
                name = name,
                status = true
            };
            await _instrumentRestAPI.CreateInstrumentAsync(instrument);
            return true;
        }

        public async Task<Instrument> DeleteInstrumentAsync(Instrument instrument, string deleteConfirmation)
        {
            if (!InputValidationUtility.DeleteConfirmation(deleteConfirmation)) return null;

            return await _instrumentRestAPI.DeleteInstrumentAsync(instrument);
        }

        private async Task<bool> IsAddNewInstrumentNameValid(string name)
        {
            var isNameNull = string.IsNullOrWhiteSpace(name);

            var instruments = await GetAllInstrumentsAsync();
            var isDuplicate = instruments.Any(x => x.name == name);

            return !isNameNull && !isDuplicate;
        }

        public async Task<List<Instrument>> GetAllActiveInstrument()
        {
            string filter = "?status=true";
            string sort = "&sortBy=Name&isAscending=true";

            return await _instrumentRestAPI.GetInstrumentsAsync(filter, sort);
        }

        public async Task<List<Instrument>> GetAllInstrumentsAsync(string filter = "", string sort = "")
        {
            return await _instrumentRestAPI.GetInstrumentsAsync(filter, sort);
        }

        private async Task<bool> IsUpdateInstrumentNameValid(Instrument instrument)
        {
            var instruments = await GetAllActiveInstrument();

            var isDuplicate = instruments.Any(x => x.name == instrument.name && x.id != instrument.id);
            var isNull = string.IsNullOrWhiteSpace(instrument.name);

            return !isDuplicate && !isNull;
        }

        public async Task<bool> UpdateInstrumentAsync(Instrument instrument)
        {
            if (!await IsUpdateInstrumentNameValid(instrument))
            {
                NotificationUtility.ShowError("Instrument name is invalid or already exists");
                return false;
            }
            else
            {
                await _instrumentRestAPI.UpdateInstrumentAsync(instrument);
                return true;
            }
        }

        public async Task<List<string>> GetAllActiveInstrumentName()
        {
            var instruments = await GetAllActiveInstrument();
            var instrumentName = instruments.Select(x => x.name).ToList();
            return instrumentName;
        }
    }
}
