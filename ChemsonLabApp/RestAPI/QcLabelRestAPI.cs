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

        /// <summary>
        /// Retrieves all QCLabel records from the API with optional filter and sort parameters.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the request URL.</param>
        /// <param name="sort">Optional sort string to apply to the request URL.</param>
        /// <returns>A list of QCLabel objects.</returns>
        public async Task<List<QCLabel>> GetAllQCLabelsAsync(string filter = "", string sort = "")
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

        /// <summary>
        /// Retrieves a QCLabel record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the QCLabel.</param>
        /// <returns>The QCLabel object if found; otherwise, null.</returns>
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

        /// <summary>
        /// Creates a new QCLabel record via the API.
        /// </summary>
        /// <param name="qCLabel">The QCLabel object to create.</param>
        /// <returns>The created QCLabel object as returned by the API.</returns>
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

        /// <summary>
        /// Updates an existing QCLabel record via the API.
        /// </summary>
        /// <param name="qCLabel">The QCLabel object with updated values.</param>
        /// <returns>The updated QCLabel object as returned by the API.</returns>
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

        /// <summary>
        /// Deletes a QCLabel record via the API.
        /// </summary>
        /// <param name="qCLabel">The QCLabel object to delete.</param>
        /// <returns>The deleted QCLabel object as returned by the API.</returns>
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
