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
    public class DailyQcRestAPI : IDailyQcRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/DailyQc";

        public List<DailyQc> DailyQcs { get; set; }
        public DailyQc DailyQc { get; set; }
        public DailyQcRestAPI()
        {
            client = new HttpClient();
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        public async Task<List<DailyQc>> GetAllDailyQcsAsync(string filter = "", string sort = "")
        {
            DailyQcs = new List<DailyQc>();
            string url = $"{baseUrl}{filter}{sort}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                DailyQcs = JsonSerializer.Deserialize<List<DailyQc>>(data, options);
            }
            return DailyQcs;
        }

        public async Task<DailyQc> GetDailyQcByIdAsync(int id)
        {
            string url = $"{baseUrl}/{id}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                DailyQc = JsonSerializer.Deserialize<DailyQc>(data, options);
            }
            return DailyQc;
        }

        public async Task<DailyQc> CreateDailyQcAsync(DailyQc dailyQc)
        {
            string url = $"{baseUrl}";
            var json = JsonSerializer.Serialize<DailyQc>(dailyQc, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                DailyQc = JsonSerializer.Deserialize<DailyQc>(data, options);
            }
            return DailyQc;
        }

        public async Task<DailyQc> UpdateDailyQcAsync(DailyQc dailyQc)
        {
            string url = $"{baseUrl}/{dailyQc.id}";
            var json = JsonSerializer.Serialize<DailyQc>(dailyQc, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                DailyQc = JsonSerializer.Deserialize<DailyQc>(data, options);
            }
            return DailyQc;
        }

        public async Task<DailyQc> DeleteDailyQcAsync(DailyQc dailyQc)
        {
            string url = $"{baseUrl}/{dailyQc.id}";
            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                DailyQc = JsonSerializer.Deserialize<DailyQc>(data, options);
            }
            return DailyQc;
        }
    }
}
