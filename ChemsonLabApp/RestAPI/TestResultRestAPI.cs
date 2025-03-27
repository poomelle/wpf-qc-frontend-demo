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
    public class TestResultRestAPI : ITestResultRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/TestResult";
        public List<TestResult> TestResults { get; set; }
        public TestResult TestResult { get; set; }

        public TestResultRestAPI()
        {
            
            client = new HttpClient();
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        public async Task<List<TestResult>> GetAllTestResultsAsync(string filter="", string sort = "")
        {
            TestResults = new List<TestResult>();
            string url = $"{baseUrl}{filter}{sort}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                TestResults = JsonSerializer.Deserialize<List<TestResult>>(data, options);
            }
            return TestResults;
        }

        public async Task<TestResult> GetTestResultByIdAsync(int id)
        {
            string url = $"{baseUrl}/{id}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                TestResult = JsonSerializer.Deserialize<TestResult>(data, options);
            }
            return TestResult;
        }

        public async Task<TestResult> CreateTestResultAsync(TestResult testResult)
        {
            string url = $"{baseUrl}";
            var json = JsonSerializer.Serialize<TestResult>(testResult, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                TestResult = JsonSerializer.Deserialize<TestResult>(data, options);
            }
            return TestResult;
        }

        public async Task<TestResult> UpdateTestResultAsync(TestResult testResult)
        {
            string url = $"{baseUrl}/{testResult.id}";
            var json = JsonSerializer.Serialize<TestResult>(testResult, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode) 
            {
                var data = await response.Content.ReadAsStringAsync();
                TestResult = JsonSerializer.Deserialize<TestResult>(data, options);
            }
            return TestResult;
        }

        public async Task<TestResult> DeleteTestResultAsync(TestResult testResult)
        {
            string url = $"{baseUrl}/{testResult.id}";
            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                TestResult = JsonSerializer.Deserialize<TestResult>(data, options);
            }
            return TestResult;
        }
    }
}
