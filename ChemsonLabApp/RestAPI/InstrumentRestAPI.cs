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
