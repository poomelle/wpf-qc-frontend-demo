using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.ProductVM.Commands;
using ChemsonLabApp.MVVM.Views.Product;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.DialogService;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PdfSharp.Pdf.Filters;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.ProductVM
{
    [AddINotifyPropertyChangedInterface]
    public class ProductViewModel
    {
        private readonly IProductService _productService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();
        public List<string> StatusComboBox { get; set; } = new List<string> { "All", "Active", "Inactive" };
        public List<string> ComboBoxProducts { get; set; }
        public string SelectedProduct { get; set; }
        public string ComboBoxSelectedStatus { get; set; }

        // Commands
        public ShowAddNewProductView ShowAddNewProductView { get; set; }
        public ShowDeleteProductView ShowDeleteProductView { get; set; }
        public ReloadDataCommand ReloadDataCommand { get; set; }
        public IsViewModeToggleCommand IsViewModeToggleCommand { get; set; }
        public UpdateProductCommand UpdateProductCommand { get; set; }
        public ProductComboBoxSeletedCommand ProductComboBoxSeletedCommand { get; set; }
        public ProductStatusComboBoxSeletedCommand ProductStatusComboBoxSeletedCommand { get; set; }
        public ProductNameSearchCommand ProductNameSearchCommand { get; set; }
        public SelectProductChangedCommand SelectProductChangedCommand { get; set; }
        public SelectStatusChangeProductCommand SelectStatusChangeProductCommand { get; set; }

        public ProductViewModel(
            IProductService productService,
            IDialogService dialogService
            )
        {
            // services
            this._productService = productService;
            this._dialogService = dialogService;

            // commands
            ShowAddNewProductView = new ShowAddNewProductView(this);
            ReloadDataCommand = new ReloadDataCommand(this);
            IsViewModeToggleCommand = new IsViewModeToggleCommand(this);
            ShowDeleteProductView = new ShowDeleteProductView(this);
            UpdateProductCommand = new UpdateProductCommand(this);
            ProductComboBoxSeletedCommand = new ProductComboBoxSeletedCommand(this);
            ProductStatusComboBoxSeletedCommand = new ProductStatusComboBoxSeletedCommand(this);
            ProductNameSearchCommand = new ProductNameSearchCommand(this);
            SelectProductChangedCommand = new SelectProductChangedCommand(this);
            SelectStatusChangeProductCommand = new SelectStatusChangeProductCommand(this);

            // Initialize
            InitializeParameter();
        }

        public async void InitializeParameter()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                await PopulateProductComboBox();

                SelectedProduct = "All";
                ComboBoxSelectedStatus = "All";

                await GetAllProductsAsync();
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Failed to load products. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error : Failed to load products. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        private async Task PopulateProductComboBox()
        {
            ComboBoxProducts = new List<string> { "All" }.Concat(await _productService.GetAllActiveProductName()).ToList();
        }

        public async Task GetAllProductsAsync()
        {
            var sort = "&sortBy=Name&isAscending=true";
            var modifiedProduct = SelectedProduct == "All" ? "" : SelectedProduct;
            var modifiedStatus = ComboBoxSelectedStatus == "All" ? "" : ComboBoxSelectedStatus == "Active" ? "true" : "false";
            var filter = $"?name={modifiedProduct}&status={modifiedStatus}";

            var products = await _productService.LoadAllProducts(filter, sort);
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }

        public async void UpdateProductAsync(Product product)
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                var updateProduct = await _productService.UpdateProduct(product);
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Failed to update product. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error : Failed to update product. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
                InitializeParameter();
            }
        }

        public void PopupAddProductWindow()
        {
            _dialogService.ShowView("AddProduct");
        }

        public void PopupDeleteProductWindow(Product product)
        {
            _dialogService.ShowDeleteView(product);
        }

        public async void isVewModeToggle(Product product)
        {
            product.isViewMode = !product.isViewMode;
            product.isEditMode = !product.isEditMode;

            if (product.isViewMode)
            {
                await GetAllProductsAsync();
            }
        }
    }
}
