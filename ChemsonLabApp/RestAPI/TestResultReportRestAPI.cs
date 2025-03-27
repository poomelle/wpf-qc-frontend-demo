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

        public async Task<List<TestResultReport>> GetAllTestResultReportAsync(string filter="", string sort = "")
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

        public async Task<TestResultReport> CreateTestResultReportAsync(TestResultReport report)
        {
            string url = $"{baseUrl}";
            var json = JsonSerializer.Serialize<TestResultReport>(report, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                TestResultReport = JsonSerializer.Deserialize<TestResultReport>(data,options);
            }
            return TestResultReport;
        }

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
