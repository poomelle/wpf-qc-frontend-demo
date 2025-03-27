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
    public class QcLabelRestAPI : IQcLabelRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/QcLabel";

        public List<QCLabel> QcLabels { get; set; }
        public QCLabel QcLabel { get; set; }
        public QcLabelRestAPI()
        {
            client = new HttpClient();
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        public async Task<List<QCLabel>> GetAllQCLabelsAsync(string filter = "", string sort = ""  )
        {
            QcLabels = new List<QCLabel>();
            string url = $"{baseUrl}{filter}{sort}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                QcLabels = JsonSerializer.Deserialize<List<QCLabel>>(data, options);
            }
            return QcLabels;
        }

        public async Task<QCLabel> GetQCLabelByIdAsync(int id)
        {
            string url = $"{baseUrl}/{id}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                QcLabel = JsonSerializer.Deserialize<QCLabel>(data, options);
            }
            return QcLabel;
        }

        public async Task<QCLabel> CreateQCLabelAsync(QCLabel qCLabel)
        {
            string url = $"{baseUrl}";
            var json = JsonSerializer.Serialize<QCLabel>(qCLabel, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                QcLabel = JsonSerializer.Deserialize<QCLabel>(data, options);
            }
            return QcLabel;
        }

        public async Task<QCLabel> UpdateQCLabelAsync(QCLabel qCLabel)
        {
            string url = $"{baseUrl}/{qCLabel.id}";
            var json = JsonSerializer.Serialize<QCLabel>(qCLabel, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                QcLabel = JsonSerializer.Deserialize<QCLabel>(data, options);
            }
            return QcLabel;
        }

        public async Task<QCLabel> DeleteQCLabelAsync(QCLabel qCLabel)
        {
            string url = $"{baseUrl}/{qCLabel.id}";

            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                QcLabel = JsonSerializer.Deserialize<QCLabel>(data, options);
            }
            return QcLabel;
        }
    }
}
