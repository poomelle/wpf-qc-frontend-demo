using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows.Controls;
using ChemsonLabApp.RestAPI.IRestAPI;

namespace ChemsonLabApp.RestAPI
{
    public class ProductRestAPI : IProductRestAPI
    {
        HttpClient client;
        JsonSerializerOptions options;
        private string baseUrl = $"{Constants.Constants.IPAddress}/Product";
        public List<Product> Products { get; private set; }
        public Product Product { get; set; }

        public ProductRestAPI()
        {
            
            client = new HttpClient();
            options = new JsonSerializerOptions() 
            {
                WriteIndented = true,
            };

        }

        public async Task<List<Product>> GetProductsAsync(string filter = "", string sort = "")
        {
            Products = new List<Product>();
            string url = $"{baseUrl}{filter}{sort}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Products = JsonSerializer.Deserialize<List<Product>>(data, options);
            }

            return Products;
        }

        public async Task<Product> GetProductByIdAsync(int id   )
        {
            string url = $"{baseUrl}/{id}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Product = JsonSerializer.Deserialize<Product>(data, options);
            }

            return Product;
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            string json = JsonSerializer.Serialize<Product>(product);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var reponse = await client.PostAsync(baseUrl, content);
            if (reponse.IsSuccessStatusCode)
            {
                var data = await reponse.Content.ReadAsStringAsync();
                Product = JsonSerializer.Deserialize<Product>(data, options);
            }
            return Product;
        }

        public async Task<Product> UpdateProductAsync(Product product) 
        {
            string url = $"{baseUrl}/{product.id}";

            string json = JsonSerializer.Serialize<Product>(product);
            StringContent content = new StringContent (json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content?.ReadAsStringAsync();
                Product = JsonSerializer.Deserialize<Product> (data, options);
            }
            return Product;
        }

        public async Task<Product> DeleteProductAsync(Product product)
        {
            string url = $"{baseUrl}/{product.id}";

            var reponse = await client.DeleteAsync(url);
            if (reponse.IsSuccessStatusCode)
            {
                var data = await reponse.Content?.ReadAsStringAsync();
                Product = JsonSerializer.Deserialize<Product> (data, options);
            }
            return Product;
        }
    }
}
