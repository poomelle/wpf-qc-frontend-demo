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

        public async Task<List<Evaluation>> GetAllEvaluationsAsync(string filter="", string sort = "")
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
