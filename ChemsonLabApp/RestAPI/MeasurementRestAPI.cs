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

        /// <summary>
        /// Retrieves all measurements from the API with optional filter and sort parameters.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the request URL.</param>
        /// <param name="sort">Optional sort string to apply to the request URL.</param>
        /// <returns>A list of Measurement objects.</returns>
        public async Task<List<Measurement>> GetAllMeasurementsAsync(string filter = "", string sort = "")
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

        /// <summary>
        /// Retrieves a measurement by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the measurement.</param>
        /// <returns>The Measurement object if found; otherwise, null.</returns>
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

        /// <summary>
        /// Creates a new measurement by sending a POST request to the API.
        /// </summary>
        /// <param name="measurement">The Measurement object to create.</param>
        /// <returns>The created Measurement object as returned by the API.</returns>
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

        /// <summary>
        /// Updates an existing measurement by sending a PUT request to the API.
        /// </summary>
        /// <param name="measurement">The Measurement object with updated values.</param>
        /// <returns>The updated Measurement object as returned by the API.</returns>
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

        /// <summary>
        /// Deletes a measurement by sending a DELETE request to the API.
        /// </summary>
        /// <param name="measurement">The Measurement object to delete.</param>
        /// <returns>The deleted Measurement object as returned by the API.</returns>
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
