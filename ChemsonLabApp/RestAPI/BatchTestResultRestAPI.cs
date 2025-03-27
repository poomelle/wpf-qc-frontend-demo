using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI
{
    public class BatchTestResultRestAPI : IBatchTestResultRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/BatchTestResult";
        public List<BatchTestResult> BatchTestResults { get; set; }
        public BatchTestResult BatchTestResult { get; set; }
        public BatchTestResultRestAPI()
        {
            client = new HttpClient();
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        public async Task<List<BatchTestResult>> GetAllBatchTestResultsAsync(string filter="", string sort = "")
        {
            BatchTestResults = new List<BatchTestResult>();
            string url = $"{baseUrl}{filter}{sort}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                BatchTestResults = JsonSerializer.Deserialize<List<BatchTestResult>>(data, options);
            }
            return BatchTestResults;
        }

        public async Task<BatchTestResult> GetBatchTestResultById(int id)
        {
            string url = $"{baseUrl}/{id}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                BatchTestResult = JsonSerializer.Deserialize<BatchTestResult>(data, options);
            }
            return BatchTestResult;
        }

        public async Task<BatchTestResult> CreateBatchTestResultAsync(BatchTestResult batchTestResult)
        {
            string url = $"{baseUrl}";
            var json = JsonSerializer.Serialize<BatchTestResult>(batchTestResult, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                BatchTestResult = JsonSerializer.Deserialize<BatchTestResult>(data, options);
            }
            return BatchTestResult;
        }

        public async Task<BatchTestResult> UpdateBatchTestResultAsync(BatchTestResult batchTestResult)
        {
            string url = $"{baseUrl}/{batchTestResult.id}";
            var json = JsonSerializer.Serialize<BatchTestResult>(batchTestResult, options);
            StringContent content = new StringContent (json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                BatchTestResult = JsonSerializer.Deserialize<BatchTestResult>(data,options);
            }
            return BatchTestResult;
        }

        public async Task<BatchTestResult> DeleteBatchTestResultAsync(BatchTestResult batchTestResult)
        {
            string url = $"{baseUrl}/{batchTestResult.id}";

            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                BatchTestResult = JsonSerializer.Deserialize<BatchTestResult>(data,options);
            }
            return BatchTestResult;
        }
    }
}
