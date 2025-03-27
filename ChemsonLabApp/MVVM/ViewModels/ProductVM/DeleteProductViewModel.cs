using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PropertyChanged;
using ChemsonLabApp.MVVM.ViewModels.ProductVM.Commands;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using System.Net.Http;

namespace ChemsonLabApp.MVVM.ViewModels.ProductVM
{
    [AddINotifyPropertyChangedInterface]
    public class DeleteProductViewModel
    {
        private readonly IProductService _productService;

        public Product Product { get; set; }
        public string DeleteConfirm { get; set; }
        public DeletedProductCommand DeletedProductCommand { get; set; }

        public DeleteProductViewModel(IProductService productService)
        {
            this._productService = productService;
            DeletedProductCommand = new DeletedProductCommand(this);
        }

        public async void DeleteProductAsync()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                var deletedProduct = await _productService.DeleteProduct(Product, DeleteConfirm);
                if (deletedProduct != null) NotificationUtility.ShowSuccess(deletedProduct.name + " has been deleted");
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error : Internet Connection error. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error : Failed to delete product. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }
    }
}
