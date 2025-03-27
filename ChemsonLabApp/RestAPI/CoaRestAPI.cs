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
    public class CoaRestAPI : ICoaRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/Coa";

        public List<Coa> Coas { get; set; }
        public Coa Coa { get; set; }

        public CoaRestAPI()
        {
            client = new HttpClient();
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        public async Task<List<Coa>> GetAllCoasAsync(string filter = "", string sort = ""   )
        {
            Coas = new List<Coa>();
            string url = $"{baseUrl}{filter}{sort}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Coas = JsonSerializer.Deserialize<List<Coa>>(data, options);
            }
            return Coas;
        }

        public async Task<Coa> GetCoaByIdAsync(int id)
        {
            string url = $"{baseUrl}/{id}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Coa = JsonSerializer.Deserialize<Coa>(data, options);
            }
            return Coa;
        }

        public async Task<Coa> CreateCoaAsync(Coa coa)
        {
            string url = $"{baseUrl}";
            var json = JsonSerializer.Serialize<Coa>(coa, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Coa = JsonSerializer.Deserialize<Coa>(data, options);
            }
            return Coa;
        }

        public async Task<Coa> UpdateCoaAsync(Coa coa)
        {
            string url = $"{baseUrl}/{coa.id}";
            var json = JsonSerializer.Serialize<Coa>(coa, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Coa = JsonSerializer.Deserialize<Coa>(data, options);
            }
            return Coa;
        }

        public async Task<Coa> DeleteCoaAsync(Coa coa)
        {
            string url = $"{baseUrl}/{coa.id}";
            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Coa = JsonSerializer.Deserialize<Coa>(data, options);
            }
            return Coa;
        }
    }
}
