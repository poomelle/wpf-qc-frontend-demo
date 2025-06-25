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

        /// <summary>
        /// Retrieves all specifications from the API with optional filter and sort parameters.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the request URL.</param>
        /// <param name="sort">Optional sort string to apply to the request URL.</param>
        /// <returns>A list of <see cref="Specification"/> objects.</returns>
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

        /// <summary>
        /// Retrieves a specification by its unique identifier.
        /// </summary>
        /// <param name="specificationID">The ID of the specification to retrieve.</param>
        /// <returns>The <see cref="Specification"/> object if found; otherwise, null.</returns>
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

        /// <summary>
        /// Creates a new specification by sending a POST request to the API.
        /// </summary>
        /// <param name="specification">The <see cref="Specification"/> object to create.</param>
        /// <returns>The created <see cref="Specification"/> object as returned by the API.</returns>
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

        /// <summary>
        /// Updates an existing specification by sending a PUT request to the API.
        /// </summary>
        /// <param name="specification">The <see cref="Specification"/> object to update.</param>
        /// <returns>The updated <see cref="Specification"/> object as returned by the API.</returns>
        public async Task<Specification> UpdateSpecificationAsync(Specification specification)
        {
            string url = $"{baseUrl}/{specification.id}";

            var json = JsonSerializer.Serialize<Specification>(specification, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Specification = JsonSerializer.Deserialize<Specification>(data, options);
            }
            return Specification;
        }

        /// <summary>
        /// Deletes a specification by sending a DELETE request to the API.
        /// </summary>
        /// <param name="specification">The <see cref="Specification"/> object to delete.</param>
        /// <returns>The deleted <see cref="Specification"/> object as returned by the API.</returns>
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
