using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ChemsonLabApp.RestAPI
{
    public class BatchRestAPI : IBatchRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/Batch";
        public List<Batch> Batches { get; set; }
        public Batch Batch { get; set; }
        public BatchRestAPI()
        {
            
            client = new HttpClient();
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        /// <summary>
        /// Retrieves all batches from the API with optional filter and sort parameters.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the request URL.</param>
        /// <param name="sort">Optional sort string to apply to the request URL.</param>
        /// <returns>A list of Batch objects.</returns>
        public async Task<List<Batch>> GetAllBatchesAsync(string filter = "", string sort = "")
        {
            Batches = new List<Batch>();
            string url = $"{baseUrl}{filter}{sort}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Batches = JsonSerializer.Deserialize<List<Batch>>(data, options);
            }
            return Batches;
        }

        /// <summary>
        /// Retrieves a batch by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the batch.</param>
        /// <returns>The Batch object if found; otherwise, null.</returns>
        public async Task<Batch> GetBatchByIdAsync(int id)
        {
            string url = $"{baseUrl}/{id}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Batch = JsonSerializer.Deserialize<Batch>(data, options);
            }
            return Batch;
        }

        /// <summary>
        /// Creates a new batch by sending a POST request to the API.
        /// </summary>
        /// <param name="batch">The Batch object to create.</param>
        /// <returns>The created Batch object as returned by the API.</returns>
        public async Task<Batch> CreateBatchAsync(Batch batch)
        {
            string url = $"{baseUrl}";
            var json = JsonSerializer.Serialize<Batch>(batch, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Batch = JsonSerializer.Deserialize<Batch>(data, options);
            }
            return Batch;
        }

        /// <summary>
        /// Updates an existing batch by sending a PUT request to the API.
        /// </summary>
        /// <param name="batch">The Batch object with updated information.</param>
        /// <returns>The updated Batch object as returned by the API.</returns>
        public async Task<Batch> UpdateBatchAsync(Batch batch)
        {
            string url = $"{baseUrl}/{batch.id}";
            var json = JsonSerializer.Serialize<Batch>(batch, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Batch = JsonSerializer.Deserialize<Batch>(data, options);
            }
            return Batch;
        }

        /// <summary>
        /// Deletes a batch by sending a DELETE request to the API.
        /// </summary>
        /// <param name="batch">The Batch object to delete.</param>
        /// <returns>The deleted Batch object as returned by the API.</returns>
        public async Task<Batch> DeleteBatchAsync(Batch batch)
        {
            string url = $"{baseUrl}/{batch.id}";
            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Batch = JsonSerializer.Deserialize<Batch>(data, options);
            }
            return Batch;
        }
    }
}
