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
    public class ReportRestAPI : IReportRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/Report";
        public Report Report { get; set; }
        public List<Report> Reports { get; set; }
        public ReportRestAPI()
        {
            client = new HttpClient();
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        /// <summary>
        /// Retrieves all reports from the API with optional filter and sort parameters.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the request.</param>
        /// <param name="sort">Optional sort string to apply to the request.</param>
        /// <returns>A list of Report objects.</returns>
        public async Task<List<Report>> GetAllReportAsync(string filter = "", string sort = "")
        {
            Reports = new List<Report>();
            string url = $"{baseUrl}{filter}{sort}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Reports = JsonSerializer.Deserialize<List<Report>>(data, options);
            }
            return Reports;
        }

        /// <summary>
        /// Retrieves a report by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the report.</param>
        /// <returns>The Report object if found; otherwise, null.</returns>
        public async Task<Report> GetReportByIdAsync(int id)
        {
            string url = $"{baseUrl}/{id}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Report = JsonSerializer.Deserialize<Report>(data, options);
            }
            return Report;
        }

        /// <summary>
        /// Creates a new report by sending a POST request to the API.
        /// </summary>
        /// <param name="report">The Report object to create.</param>
        /// <returns>The created Report object returned by the API.</returns>
        public async Task<Report> CreateReportAsync(Report report)
        {
            string url = $"{baseUrl}";
            var json = JsonSerializer.Serialize<Report>(report, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Report = JsonSerializer.Deserialize<Report>(data, options);
            }
            return Report;
        }

        /// <summary>
        /// Updates an existing report by sending a POST request to the API.
        /// </summary>
        /// <param name="report">The Report object with updated data.</param>
        /// <returns>The updated Report object returned by the API.</returns>
        public async Task<Report> UpdateReportAsync(Report report)
        {
            string url = $"{baseUrl}/{report.id}";
            var json = JsonSerializer.Serialize<Report>(report, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Report = JsonSerializer.Deserialize<Report>(data, options);
            }
            return Report;
        }

        /// <summary>
        /// Deletes a report by its unique identifier.
        /// </summary>
        /// <param name="report">The Report object to delete.</param>
        /// <returns>The deleted Report object returned by the API.</returns>
        public async Task<Report> DeleteReportAsync(Report report)
        {
            string url = $"{baseUrl}/{report.id}";
            var resoponse = await client.DeleteAsync(url);
            if (resoponse.IsSuccessStatusCode)
            {
                var data = await resoponse.Content.ReadAsStringAsync();
                Report = JsonSerializer.Deserialize<Report>(data, options);
            }
            return Report;
        }
    }
}
