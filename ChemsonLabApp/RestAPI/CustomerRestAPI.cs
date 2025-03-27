using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ChemsonLabApp.RestAPI
{
    public class CustomerRestAPI : ICustomerRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        public List<Customer> Customers { get; set; }
        public Customer Customer { get; set; }
        private string baseUrl = $"{Constants.Constants.IPAddress}/Customer";
        public CustomerRestAPI()
        {
            client = new HttpClient();
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        public async Task<List<Customer>> GetAllCustomersAsync(string filter="", string sort = "")
        {
            string url = $"{baseUrl}{filter}{sort}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Customers = JsonSerializer.Deserialize<List<Customer>>(data, options);
            }
            return Customers;
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            string url = $"{baseUrl}/{id}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Customer = JsonSerializer.Deserialize<Customer>(data, options);
            }
            return Customer;
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            string url = $"{baseUrl}";
            var json = JsonSerializer.Serialize<Customer>(customer, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Customer = JsonSerializer.Deserialize<Customer>(data, options);
            }
            return Customer;
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            string url = $"{baseUrl}/{customer.id}";
            var json = JsonSerializer.Serialize<Customer>(customer, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Customer = JsonSerializer.Deserialize<Customer>(data, options);
            }
            return Customer;
        }

        public async Task<Customer> DeleteCustomerAsync(Customer customer)
        {
            string url = $"{baseUrl}/{customer.id}";

            var reponse = await client.DeleteAsync(url);
            if (reponse.IsSuccessStatusCode)
            {
                var data = await reponse.Content.ReadAsStringAsync();
                Customer = JsonSerializer.Deserialize<Customer>(data, options);
            }
            return Customer;
        }
    }
}
