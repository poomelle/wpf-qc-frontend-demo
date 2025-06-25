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
    public class EvaluationRestAPI : IEvaluationRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/Evaluation";
        public List<Evaluation> Evaluations { get; set; }
        public Evaluation Evaluation { get; set; }
        public EvaluationRestAPI()
        {
            client = new HttpClient();
            options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        /// <summary>
        /// Retrieves all evaluations from the API with optional filter and sort parameters.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the request URL.</param>
        /// <param name="sort">Optional sort string to apply to the request URL.</param>
        /// <returns>A list of Evaluation objects.</returns>
        public async Task<List<Evaluation>> GetAllEvaluationsAsync(string filter = "", string sort = "")
        {
            Evaluations = new List<Evaluation>();
            string url = $"{baseUrl}{filter}{sort}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Evaluations = JsonSerializer.Deserialize<List<Evaluation>>(data, options);
            }
            return Evaluations;
        }

        /// <summary>
        /// Retrieves a single evaluation by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the evaluation.</param>
        /// <returns>The Evaluation object if found; otherwise, null.</returns>
        public async Task<Evaluation> GetEvaluationByIdAsync(int id)
        {
            string url = $"{baseUrl}/{id}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Evaluation = JsonSerializer.Deserialize<Evaluation>(data, options);
            }
            return Evaluation;
        }

        /// <summary>
        /// Creates a new evaluation by sending a POST request to the API.
        /// </summary>
        /// <param name="evaluation">The Evaluation object to create.</param>
        /// <returns>The created Evaluation object as returned by the API.</returns>
        public async Task<Evaluation> CreateEvaluationAsync(Evaluation evaluation)
        {
            string url = $"{baseUrl}";
            var json = JsonSerializer.Serialize<Evaluation>(evaluation, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Evaluation = JsonSerializer.Deserialize<Evaluation>(data, options);
            }
            return Evaluation;
        }

        /// <summary>
        /// Updates an existing evaluation by sending a PUT request to the API.
        /// </summary>
        /// <param name="evaluation">The Evaluation object with updated data.</param>
        /// <returns>The updated Evaluation object as returned by the API.</returns>
        public async Task<Evaluation> UpdateEvaluationAsync(Evaluation evaluation)
        {
            string url = $"{baseUrl}/{evaluation.id}";
            var json = JsonSerializer.Serialize<Evaluation>(evaluation, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Evaluation = JsonSerializer.Deserialize<Evaluation>(data, options);
            }
            return Evaluation;
        }

        /// <summary>
        /// Deletes an evaluation by sending a DELETE request to the API.
        /// </summary>
        /// <param name="evaluation">The Evaluation object to delete.</param>
        /// <returns>The deleted Evaluation object as returned by the API.</returns>
        public async Task<Evaluation> DeleteEvaluationAsync(Evaluation evaluation)
        {
            string url = $"{baseUrl}/{evaluation.id}";
            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Evaluation = JsonSerializer.Deserialize<Evaluation>(data, options);
            }
            return Evaluation;
        }
    }   
}
