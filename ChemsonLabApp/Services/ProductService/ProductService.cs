using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRestAPI _productRestAPI;
        private readonly ISpecificationService _specificationService;

        public ProductService(IProductRestAPI productRestAPI, ISpecificationService specificationService)
        {
            this._productRestAPI = productRestAPI;
            this._specificationService = specificationService;
        }


        public async Task<List<Product>> LoadActiveProducts()
        {
            string filter = "?status=true";
            string sort = "&sortBy=Name&isAscending=true";

            return await _productRestAPI.GetProductsAsync(filter, sort);
        }

        public async Task<List<Product>> LoadAllProducts(string filer="", string sort="")
        {
            return await _productRestAPI.GetProductsAsync(filer, sort);
        }

        public async Task<Product> CreateNewProductByProductName(string name)
        {
            var isNewProductNameValid = await IsNewProductNameValid(name);

            if (!isNewProductNameValid)
            {
                NotificationUtility.ShowError("Invalid product name or already exists");
                return null;
            }

            var product = new Product
            {
                name = name,
                status = true
            };

            return await _productRestAPI.CreateProductAsync(product);
        }

        private async Task<bool> IsNewProductNameValid(string name)
        {
            var isProductNameNull = string.IsNullOrWhiteSpace(name);
            var products = await LoadAllProducts();
            var isDuplicate = products.Any(x => x.name == name);

            return !isProductNameNull && !isDuplicate;
        }

        public async Task<Product> UpdateProduct(Product product)
        {
            var isProductNameValid = await IsUpdateProductNameValid(product);

            if (!isProductNameValid)
            {
                NotificationUtility.ShowError("Product name is invalid or already exists");
                return null;
            }

            var updateProduct = await _productRestAPI.UpdateProductAsync(product);

            await _specificationService.UpdateSpecificationFromProductUpdated(updateProduct);

            return updateProduct;
        }

        private async Task<bool> IsUpdateProductNameValid(Product product)
        {
            var isProductNameNull = string.IsNullOrWhiteSpace(product.name);
            var products = await LoadAllProducts();
            var isUpdateNameDuplicate = products.Any(x => x.name == product.name && x.id != product.id);

            return !isProductNameNull && !isUpdateNameDuplicate;
        }

        public async Task<Product> DeleteProduct(Product product, string deleteConfirmation)
        {
            if (!InputValidationUtility.DeleteConfirmation(deleteConfirmation)) return null;

            return await _productRestAPI.DeleteProductAsync(product);
        }

        public async Task<Product> GetProductFromProductName(string productName)
        {
            var filter = $"?name={productName}";
            var products = await _productRestAPI.GetProductsAsync(filter);
            return products.FirstOrDefault();
        }

        public async Task<List<string>> GetAllActiveProductName()
        {
            var products = await LoadActiveProducts();
            var productNames = products.Select(p => p.name).ToList();
            return productNames;
        }
    }
}
