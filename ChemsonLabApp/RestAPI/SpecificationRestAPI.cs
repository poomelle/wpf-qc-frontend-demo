using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ChemsonLabApp.RestAPI
{
    public class SpecificationRestAPI : ISpecificationRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/ProductSpecification";
        public List<Specification> Specifications { get; set; }
        public Specification Specification { get; set; }

        public SpecificationRestAPI()
        {
            
            client = new HttpClient();
            options = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };
        }

        public async Task<List<Specification>> GetAllSpecificationsAsync(string filter = "", string sort = "")
        {
            Specifications = new List<Specification>();
            string url = $"{baseUrl}{filter}{sort}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Specifications = JsonSerializer.Deserialize<List<Specification>>(data, options);
            }
            return Specifications;
        }

        public async Task<Specification> GetSpecificationByIdAsync(int specificationID)
        {
            string url = $"{baseUrl}/{specificationID}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Specification = JsonSerializer.Deserialize<Specification>(data, options);
            }
            return Specification;
        }

        public async Task<Specification> CreateSpecificationAsync(Specification specification)
        {
            var json = JsonSerializer.Serialize<Specification>(specification, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(baseUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Specification = JsonSerializer.Deserialize<Specification>(data, options);
            }
            return Specification;
        }

        public async Task<Specification> UpdateSpecificationAsync(Specification specification)
        {
            string url = $"{baseUrl}/{specification.id}";

            var json = JsonSerializer.Serialize<Specification>(specification, options);
            StringContent content = new StringContent (json, Encoding.UTF8, "application/json");

            var response =await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Specification = JsonSerializer.Deserialize<Specification>(data, options);
            }
            return Specification;
        }

        public async Task<Specification> DeleteSpecificationAsync(Specification specification)
        {
            string url = $"{baseUrl}/{specification.id}";

            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Specification = JsonSerializer.Deserialize<Specification>(data, options);
            }
            return Specification;
        }
    }
}
