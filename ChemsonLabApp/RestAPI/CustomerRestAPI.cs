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

        /// <summary>
        /// Retrieves all customers from the API with optional filter and sort parameters.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the request.</param>
        /// <param name="sort">Optional sort string to apply to the request.</param>
        /// <returns>A list of Customer objects.</returns>
        public async Task<List<Customer>> GetAllCustomersAsync(string filter = "", string sort = "")
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

        /// <summary>
        /// Retrieves a customer by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the customer.</param>
        /// <returns>The Customer object if found; otherwise, null.</returns>
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

        /// <summary>
        /// Creates a new customer using the provided customer object.
        /// </summary>
        /// <param name="customer">The Customer object to create.</param>
        /// <returns>The created Customer object returned from the API.</returns>
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

        /// <summary>
        /// Updates an existing customer using the provided customer object.
        /// </summary>
        /// <param name="customer">The Customer object with updated information.</param>
        /// <returns>The updated Customer object returned from the API.</returns>
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

        /// <summary>
        /// Deletes a customer using the provided customer object.
        /// </summary>
        /// <param name="customer">The Customer object to delete.</param>
        /// <returns>The deleted Customer object returned from the API.</returns>
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
