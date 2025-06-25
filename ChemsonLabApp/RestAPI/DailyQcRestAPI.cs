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
    public class DailyQcRestAPI : IDailyQcRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/DailyQc";

        public List<DailyQc> DailyQcs { get; set; }
        public DailyQc DailyQc { get; set; }
        public DailyQcRestAPI()
        {
            client = new HttpClient();
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        /// <summary>
        /// Retrieves all DailyQc records from the API with optional filter and sort parameters.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the request URL.</param>
        /// <param name="sort">Optional sort string to apply to the request URL.</param>
        /// <returns>A list of DailyQc objects.</returns>
        public async Task<List<DailyQc>> GetAllDailyQcsAsync(string filter = "", string sort = "")
        {
            DailyQcs = new List<DailyQc>();
            string url = $"{baseUrl}{filter}{sort}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                DailyQcs = JsonSerializer.Deserialize<List<DailyQc>>(data, options);
            }
            return DailyQcs;
        }

        /// <summary>
        /// Retrieves a DailyQc record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the DailyQc record.</param>
        /// <returns>The DailyQc object if found; otherwise, null.</returns>
        public async Task<DailyQc> GetDailyQcByIdAsync(int id)
        {
            string url = $"{baseUrl}/{id}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                DailyQc = JsonSerializer.Deserialize<DailyQc>(data, options);
            }
            return DailyQc;
        }

        /// <summary>
        /// Creates a new DailyQc record via the API.
        /// </summary>
        /// <param name="dailyQc">The DailyQc object to create.</param>
        /// <returns>The created DailyQc object as returned by the API.</returns>
        public async Task<DailyQc> CreateDailyQcAsync(DailyQc dailyQc)
        {
            string url = $"{baseUrl}";
            var json = JsonSerializer.Serialize<DailyQc>(dailyQc, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                DailyQc = JsonSerializer.Deserialize<DailyQc>(data, options);
            }
            return DailyQc;
        }

        /// <summary>
        /// Updates an existing DailyQc record via the API.
        /// </summary>
        /// <param name="dailyQc">The DailyQc object with updated values.</param>
        /// <returns>The updated DailyQc object as returned by the API.</returns>
        public async Task<DailyQc> UpdateDailyQcAsync(DailyQc dailyQc)
        {
            string url = $"{baseUrl}/{dailyQc.id}";
            var json = JsonSerializer.Serialize<DailyQc>(dailyQc, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                DailyQc = JsonSerializer.Deserialize<DailyQc>(data, options);
            }
            return DailyQc;
        }

        /// <summary>
        /// Deletes a DailyQc record via the API.
        /// </summary>
        /// <param name="dailyQc">The DailyQc object to delete.</param>
        /// <returns>The deleted DailyQc object as returned by the API.</returns>
        public async Task<DailyQc> DeleteDailyQcAsync(DailyQc dailyQc)
        {
            string url = $"{baseUrl}/{dailyQc.id}";
            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                DailyQc = JsonSerializer.Deserialize<DailyQc>(data, options);
            }
            return DailyQc;
        }
    }
}
