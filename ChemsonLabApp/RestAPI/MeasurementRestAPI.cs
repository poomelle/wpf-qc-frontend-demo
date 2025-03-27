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
    public class MeasurementRestAPI : IMeasurementRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/Measurement";
        public List<Measurement> Measurements { get; set; }
        public Measurement Measurement { get; set; }
        public MeasurementRestAPI()
        {
            client = new HttpClient();
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        public async Task<List<Measurement>> GetAllMeasurementsAsync(string filter="", string sort = "")
        {
            string url = $"{baseUrl}{filter}{sort}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Measurements = JsonSerializer.Deserialize<List<Measurement>>(data, options);
            }
            return Measurements;
        }

        public async Task<Measurement> GetMeasurementByIdAsync(int id)
        {
            string url = $"{baseUrl}/{id}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Measurement = JsonSerializer.Deserialize<Measurement>(data, options);
            }
            return Measurement;
        }

        public async Task<Measurement> CreateMeasurementAsync(Measurement measurement)
        {
            string url = $"{baseUrl}";
            var json = JsonSerializer.Serialize<Measurement>(measurement, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Measurement = JsonSerializer.Deserialize<Measurement>(data, options);
            }
            return Measurement;
        }

        public async Task<Measurement> UpdateMeasurementAsync(Measurement measurement)
        {
            string url = $"{baseUrl}/{measurement.id}";
            var json = JsonSerializer.Serialize<Measurement>(measurement, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Measurement = JsonSerializer.Deserialize<Measurement>(data, options);
            }
            return Measurement;
        }

        public async Task<Measurement> DeleteMeasurementAsync(Measurement measurement)
        {
            string url = $"{baseUrl}/{measurement.id}";

            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Measurement = JsonSerializer.Deserialize<Measurement>(data, options);
            }
            return Measurement;
        }
    }
}
