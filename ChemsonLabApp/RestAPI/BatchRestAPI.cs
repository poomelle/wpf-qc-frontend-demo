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
