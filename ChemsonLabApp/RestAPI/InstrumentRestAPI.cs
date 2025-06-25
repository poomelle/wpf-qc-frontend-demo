using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI
{
    public class InstrumentRestAPI : IInstrumentRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/Machine";
        public List<Instrument> Instruments { get; private set; }
        public Instrument Instrument { get; set; }

        public InstrumentRestAPI()
        {
            
            client = new HttpClient();
            options = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };
        }

        /// <summary>
        /// Retrieves a list of instruments from the REST API, with optional filtering and sorting.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the request URL.</param>
        /// <param name="sort">Optional sort string to apply to the request URL.</param>
        /// <returns>A list of <see cref="Instrument"/> objects.</returns>
        public async Task<List<Instrument>> GetInstrumentsAsync(string filter = "", string sort = "")
        {
            Instruments = new List<Instrument>();
            string url = $"{baseUrl}{filter}{sort}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Instruments = JsonSerializer.Deserialize<List<Instrument>>(data, options);
            }

            return Instruments;
        }

        /// <summary>
        /// Retrieves a single instrument by its ID from the REST API.
        /// </summary>
        /// <param name="id">The ID of the instrument to retrieve.</param>
        /// <returns>The <see cref="Instrument"/> object if found; otherwise, null.</returns>
        public async Task<Instrument> GetInstrumentByIdAsync(int id)
        {
            string url = $"{baseUrl}/{id}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Instrument = JsonSerializer.Deserialize<Instrument>(data, options);
            }

            return Instrument;
        }

        /// <summary>
        /// Creates a new instrument by sending a POST request to the REST API.
        /// </summary>
        /// <param name="instrument">The <see cref="Instrument"/> object to create.</param>
        /// <returns>The created <see cref="Instrument"/> object as returned by the API.</returns>
        public async Task<Instrument> CreateInstrumentAsync(Instrument instrument)
        {
            var json = JsonSerializer.Serialize<Instrument>(instrument, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(baseUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Instrument = JsonSerializer.Deserialize<Instrument>(data, options);
            }
            return Instrument;
        }

        /// <summary>
        /// Updates an existing instrument by sending a PUT request to the REST API.
        /// </summary>
        /// <param name="instrument">The <see cref="Instrument"/> object to update.</param>
        /// <returns>The updated <see cref="Instrument"/> object as returned by the API.</returns>
        public async Task<Instrument> UpdateInstrumentAsync(Instrument instrument)
        {
            string url = $"{baseUrl}/{instrument.id}";

            var json = JsonSerializer.Serialize<Instrument>(instrument, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Instrument = JsonSerializer.Deserialize<Instrument>(data, options);
            }
            return Instrument;
        }

        /// <summary>
        /// Deletes an instrument by sending a DELETE request to the REST API.
        /// </summary>
        /// <param name="instrument">The <see cref="Instrument"/> object to delete.</param>
        /// <returns>The deleted <see cref="Instrument"/> object as returned by the API.</returns>
        public async Task<Instrument> DeleteInstrumentAsync(Instrument instrument)
        {
            string url = $"{baseUrl}/{instrument.id}";

            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Instrument = JsonSerializer.Deserialize<Instrument>(data, options);
            }
            return Instrument;
        }
    }
}
