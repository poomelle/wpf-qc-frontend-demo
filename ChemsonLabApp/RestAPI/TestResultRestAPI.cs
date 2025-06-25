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

        /// <summary>
        /// Retrieves all test results from the API with optional filter and sort parameters.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the request URL.</param>
        /// <param name="sort">Optional sort string to apply to the request URL.</param>
        /// <returns>A list of <see cref="TestResult"/> objects.</returns>
        public async Task<List<TestResult>> GetAllTestResultsAsync(string filter = "", string sort = "")
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

        /// <summary>
        /// Retrieves a single test result by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the test result.</param>
        /// <returns>A <see cref="TestResult"/> object if found; otherwise, null.</returns>
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

        /// <summary>
        /// Creates a new test result by sending a POST request to the API.
        /// </summary>
        /// <param name="testResult">The <see cref="TestResult"/> object to create.</param>
        /// <returns>The created <see cref="TestResult"/> object as returned by the API.</returns>
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

        /// <summary>
        /// Updates an existing test result by sending a PUT request to the API.
        /// </summary>
        /// <param name="testResult">The <see cref="TestResult"/> object to update.</param>
        /// <returns>The updated <see cref="TestResult"/> object as returned by the API.</returns>
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

        /// <summary>
        /// Deletes a test result by sending a DELETE request to the API.
        /// </summary>
        /// <param name="testResult">The <see cref="TestResult"/> object to delete.</param>
        /// <returns>The deleted <see cref="TestResult"/> object as returned by the API.</returns>
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
