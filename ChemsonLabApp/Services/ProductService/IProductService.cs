using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.IService
{
    public interface IProductService
    {
        Task<List<Product>> LoadActiveProducts();
        Task<List<Product>> LoadAllProducts(string filter = "", string sort = "");
        Task<Product> CreateNewProductByProductName(string name);
        Task<Product> UpdateProduct(Product product);
        Task<Product> DeleteProduct(Product product, string deleteConfirmation);
        Task<Product> GetProductFromProductName(string productName);
        Task<List<string>> GetAllActiveProductName();
    }
}
