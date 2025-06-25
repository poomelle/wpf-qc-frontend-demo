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

namespace ChemsonLabApp.RestAPI
{
    public class CustomerOrderRestAPI : ICustomerOrderRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/CustomerOrder";
        public List<CustomerOrder> CustomerOrders { get; set; }
        public CustomerOrder CustomerOrder { get; set; }

        public CustomerOrderRestAPI()
        {
            client = new HttpClient();
            options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        /// <summary>
        /// Retrieves all customer orders from the API with optional filter and sort parameters.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the request URL.</param>
        /// <param name="sort">Optional sort string to apply to the request URL.</param>
        /// <returns>A list of <see cref="CustomerOrder"/> objects.</returns>
        public async Task<List<CustomerOrder>> GetAllCustomerOrdersAsync(string filter = "", string sort = "")
        {
            string url = $"{baseUrl}{filter}{sort}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                CustomerOrders = JsonSerializer.Deserialize<List<CustomerOrder>>(data, options);
            }
            return CustomerOrders;
        }

        /// <summary>
        /// Retrieves a customer order by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the customer order.</param>
        /// <returns>The <see cref="CustomerOrder"/> object if found; otherwise, null.</returns>
        public async Task<CustomerOrder> GetCustomerOrderByIdAsync(int id)
        {
            string url = $"{baseUrl}/{id}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                CustomerOrder = JsonSerializer.Deserialize<CustomerOrder>(data, options);
            }
            return CustomerOrder;
        }

        /// <summary>
        /// Creates a new customer order via the API.
        /// </summary>
        /// <param name="customerOrder">The <see cref="CustomerOrder"/> object to create.</param>
        /// <returns>The created <see cref="CustomerOrder"/> object as returned by the API.</returns>
        public async Task<CustomerOrder> CreateCustomerOrderAsync(CustomerOrder customerOrder)
        {
            string url = $"{baseUrl}";
            var json = JsonSerializer.Serialize<CustomerOrder>(customerOrder);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                CustomerOrder = JsonSerializer.Deserialize<CustomerOrder>(data, options);
            }
            return CustomerOrder;
        }

        /// <summary>
        /// Updates an existing customer order via the API.
        /// </summary>
        /// <param name="customerOrder">The <see cref="CustomerOrder"/> object to update.</param>
        /// <returns>The updated <see cref="CustomerOrder"/> object as returned by the API.</returns>
        public async Task<CustomerOrder> UpdateCustomerOrderAsync(CustomerOrder customerOrder)
        {
            string url = $"{baseUrl}/{customerOrder.id}";
            var json = JsonSerializer.Serialize<CustomerOrder>(customerOrder, options);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                CustomerOrder = JsonSerializer.Deserialize<CustomerOrder>(data, options);
            }
            return CustomerOrder;
        }

        /// <summary>
        /// Deletes a customer order via the API.
        /// </summary>
        /// <param name="customerOrder">The <see cref="CustomerOrder"/> object to delete.</param>
        /// <returns>The deleted <see cref="CustomerOrder"/> object as returned by the API.</returns>
        public async Task<CustomerOrder> DeleteCustomerOrderAsync(CustomerOrder customerOrder)
        {
            string url = $"{baseUrl}/{customerOrder.id}";

            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                CustomerOrder = JsonSerializer.Deserialize<CustomerOrder>(data, options);
            }
            return CustomerOrder;
        }
    }
}
