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

        /// <summary>
        /// Creates a new instrument with the specified name after validating that the name is not null, empty, or a duplicate.
        /// Shows an error notification if the name is invalid or already exists.
        /// </summary>
        /// <param name="name">The name of the instrument to create.</param>
        /// <returns>True if the instrument was created successfully; otherwise, false.</returns>
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

        /// <summary>
        /// Deletes the specified instrument if the delete confirmation is valid.
        /// </summary>
        /// <param name="instrument">The instrument to delete.</param>
        /// <param name="deleteConfirmation">The confirmation string for deletion.</param>
        /// <returns>The deleted instrument if successful; otherwise, null.</returns>
        public async Task<Instrument> DeleteInstrumentAsync(Instrument instrument, string deleteConfirmation)
        {
            if (!InputValidationUtility.DeleteConfirmation(deleteConfirmation)) return null;

            return await _instrumentRestAPI.DeleteInstrumentAsync(instrument);
        }

        /// <summary>
        /// Validates whether a new instrument name is not null, empty, or a duplicate of an existing instrument.
        /// </summary>
        /// <param name="name">The instrument name to validate.</param>
        /// <returns>True if the name is valid; otherwise, false.</returns>
        private async Task<bool> IsAddNewInstrumentNameValid(string name)
        {
            var isNameNull = string.IsNullOrWhiteSpace(name);

            var instruments = await GetAllInstrumentsAsync();
            var isDuplicate = instruments.Any(x => x.name == name);

            return !isNameNull && !isDuplicate;
        }

        /// <summary>
        /// Retrieves all active instruments, sorted by name in ascending order.
        /// </summary>
        /// <returns>A list of all active instruments.</returns>
        public async Task<List<Instrument>> GetAllActiveInstrument()
        {
            string filter = "?status=true";
            string sort = "&sortBy=Name&isAscending=true";

            return await _instrumentRestAPI.GetInstrumentsAsync(filter, sort);
        }

        /// <summary>
        /// Retrieves all instruments with optional filtering and sorting.
        /// </summary>
        /// <param name="filter">The filter string to apply.</param>
        /// <param name="sort">The sort string to apply.</param>
        /// <returns>A list of instruments matching the filter and sort criteria.</returns>
        public async Task<List<Instrument>> GetAllInstrumentsAsync(string filter = "", string sort = "")
        {
            return await _instrumentRestAPI.GetInstrumentsAsync(filter, sort);
        }

        /// <summary>
        /// Validates whether the instrument name can be updated (not null, empty, or a duplicate of another active instrument).
        /// </summary>
        /// <param name="instrument">The instrument to validate.</param>
        /// <returns>True if the update is valid; otherwise, false.</returns>
        private async Task<bool> IsUpdateInstrumentNameValid(Instrument instrument)
        {
            var instruments = await GetAllActiveInstrument();

            var isDuplicate = instruments.Any(x => x.name == instrument.name && x.id != instrument.id);
            var isNull = string.IsNullOrWhiteSpace(instrument.name);

            return !isDuplicate && !isNull;
        }

        /// <summary>
        /// Updates the specified instrument after validating the name.
        /// Shows an error notification if the name is invalid or already exists.
        /// </summary>
        /// <param name="instrument">The instrument to update.</param>
        /// <returns>True if the instrument was updated successfully; otherwise, false.</returns>
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

        /// <summary>
        /// Retrieves the names of all active instruments.
        /// </summary>
        /// <returns>A list of names of all active instruments.</returns>
        public async Task<List<string>> GetAllActiveInstrumentName()
        {
            var instruments = await GetAllActiveInstrument();
            var instrumentName = instruments.Select(x => x.name).ToList();
            return instrumentName;
        }
    }
}
