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
    public class TestResultReportRestAPI : ITestResultReportRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/TestResultReport";

        public List<TestResultReport> TestResultReports { get; set; }
        public TestResultReport TestResultReport { get; set; }
        public TestResultReportRestAPI()
        {
            client = new HttpClient();
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        /// <summary>
        /// Retrieves all test result reports asynchronously, with optional filtering and sorting.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the request.</param>
        /// <param name="sort">Optional sort string to apply to the request.</param>
        /// <returns>A list of <see cref="TestResultReport"/> objects.</returns>
        public async Task<List<TestResultReport>> GetAllTestResultReportAsync(string filter = "", string sort = "")
        {
            TestResultReports = new List<TestResultReport>();
            string url = $"{baseUrl}{filter}{sort}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                TestResultReports = JsonSerializer.Deserialize<List<TestResultReport>>(data, options);
            }
            return TestResultReports;
        }

        /// <summary>
        /// Retrieves a test result report by its unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the test result report.</param>
        /// <returns>The <see cref="TestResultReport"/> object if found; otherwise, null.</returns>
        public async Task<TestResultReport> GetTestResultReportByIdAsync(int id)
        {
            string url = $"{baseUrl}/{id}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                TestResultReport = JsonSerializer.Deserialize<TestResultReport>(data, options);
            }
            return TestResultReport;
        }

        /// <summary>
        /// Creates a new test result report asynchronously.
        /// </summary>
        /// <param name="report">The <see cref="TestResultReport"/> object to create.</param>
        /// <returns>The created <see cref="TestResultReport"/> object.</returns>
        public async Task<TestResultReport> CreateTestResultReportAsync(TestResultReport report)
        {
            string url = $"{baseUrl}";
            var json = JsonSerializer.Serialize<TestResultReport>(report, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                TestResultReport = JsonSerializer.Deserialize<TestResultReport>(data, options);
            }
            return TestResultReport;
        }

        /// <summary>
        /// Updates an existing test result report asynchronously.
        /// </summary>
        /// <param name="resultReport">The <see cref="TestResultReport"/> object to update.</param>
        /// <returns>The updated <see cref="TestResultReport"/> object.</returns>
        public async Task<TestResultReport> UpdateTestResultReportAsync(TestResultReport resultReport)
        {
            string url = $"{baseUrl}/{resultReport.id}";
            var json = JsonSerializer.Serialize<TestResultReport>(resultReport, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                TestResultReport = JsonSerializer.Deserialize<TestResultReport>(data, options);
            }
            return TestResultReport;
        }

        /// <summary>
        /// Deletes a test result report asynchronously.
        /// </summary>
        /// <param name="testResultReport">The <see cref="TestResultReport"/> object to delete.</param>
        /// <returns>The deleted <see cref="TestResultReport"/> object.</returns>
        public async Task<TestResultReport> DeleteTestResultReportAsync(TestResultReport testResultReport)
        {
            string url = $"{baseUrl}/{testResultReport.id}";

            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                TestResultReport = JsonSerializer.Deserialize<TestResultReport>(data, options);
            }
            return TestResultReport;
        }
    }
}
