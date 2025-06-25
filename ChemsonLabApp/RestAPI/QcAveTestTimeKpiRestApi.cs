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
    public class QcAveTestTimeKpiRestApi : IQcAveTimeKpiRestApi
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/QcAveTestTimeKpi";

        public QcAveTestTimeKpiRestApi()
        {
            client = new HttpClient();
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        /// <summary>
        /// Retrieves all QC average test time KPI records asynchronously from the REST API.
        /// Optional filter and sort parameters can be provided to refine the results.
        /// </summary>
        /// <param name="filter">A string representing filter criteria to be appended to the request URL.</param>
        /// <param name="sort">A string representing sort criteria to be appended to the request URL.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of QcAveTestTimeKpi objects.</returns>
        public async Task<List<QcAveTestTimeKpi>> GetAllQcAveTestTimeKpisAsync(string filter = "", string sort = "")
        {
            List<QcAveTestTimeKpi> qcAveTestTimeKpis = new List<QcAveTestTimeKpi>();
            string url = $"{baseUrl}{filter}{sort}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                qcAveTestTimeKpis = JsonSerializer.Deserialize<List<QcAveTestTimeKpi>>(data, options);
            }
            return qcAveTestTimeKpis;
        }
    }
}
