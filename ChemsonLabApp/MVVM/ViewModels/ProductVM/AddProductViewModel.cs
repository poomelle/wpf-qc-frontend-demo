using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.ProductVM.Commands;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.ProductVM
{
    [AddINotifyPropertyChangedInterface]
    public class AddProductViewModel
    {
        private readonly IProductService _productService;

        public string ProductName { get; set;}
        public List<Product> Products { get; set; } = new List<Product>();
        public SaveNewProductCommand SaveNewProductCommand { get; set; }
        public AddProductViewModel(
            IProductService productService
            )
        {
            // services
            this._productService = productService;

            // commands
            SaveNewProductCommand = new SaveNewProductCommand(this);
        }

        public async void AddNewProduct()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                var createdProduct = await _productService.CreateNewProductByProductName(ProductName);
                if (createdProduct != null) NotificationUtility.ShowSuccess(createdProduct.name + " has been created");
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error : Internet Connection error. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error : Failed to save new product. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }
    }
}
