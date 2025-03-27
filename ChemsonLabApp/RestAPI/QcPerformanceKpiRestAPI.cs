using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI
{
    public class QcPerformanceKpiRestAPI : IQcPerformanceKpiRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/QcPerformanceKpi";

        public QcPerformanceKpiRestAPI()
        {
            client = new HttpClient();
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        public async Task<List<QcPerformanceKpi>> GetAllQcPerformanceKpisAsync(string filter = "", string sort = "")
        {
            List<QcPerformanceKpi> qcPerformanceKpis = new List<QcPerformanceKpi>();
            string url = $"{baseUrl}{filter}{sort}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                qcPerformanceKpis = JsonSerializer.Deserialize<List<QcPerformanceKpi>>(data, options);
            }
            return qcPerformanceKpis;
        }
    }
}
