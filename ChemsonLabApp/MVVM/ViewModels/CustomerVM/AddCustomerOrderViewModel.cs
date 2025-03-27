using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.CustomerVM.Commands;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.CustomerVM
{
    [AddINotifyPropertyChangedInterface]
    public class AddCustomerOrderViewModel
    {
        private readonly IProductService _productService;
        private readonly ICustomerViewService _customerService;
        private readonly ICustomerOrderService _customerOrderService;

        public bool isPairProduct { get; set; } = false;
        public bool useExistingCustomer { get; set; } = true;
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public Product SelectedProduct { get; set; }
        public Customer SelectedCustomer { get; set; }
        public string NewCustomerName { get; set; }
        public string NewCustomerEmail { get; set; }
        public SaveCustomerOrderCommand SaveCustomerOrderCommand { get; set; }

        public AddCustomerOrderViewModel(IProductService productService, ICustomerViewService customerService, ICustomerOrderService customerOrderService)
        {
            // commands
            SaveCustomerOrderCommand = new SaveCustomerOrderCommand(this);

            // services
            this._productService = productService;
            this._customerService = customerService;
            this._customerOrderService = customerOrderService;

            InitializeParameters();
        }

        private async void InitializeParameters()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                Products = await _productService.LoadActiveProducts();
                Customers = await _customerService.LoadActiveCustomers();
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Failed to connect to server. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("An error occurred. Please try again.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        public async Task SaveCustomerOrderAsync()
        {
            Customer customer = new Customer();

            if (useExistingCustomer)
            {
                customer = SelectedCustomer;
            }
            else
            {
                customer = await _customerService.CreateCustomerAsync(NewCustomerName, NewCustomerEmail);
            }

            await CreateAndNotifyCustomerOrderAsync(customer);
        }

        private async Task CreateAndNotifyCustomerOrderAsync(Customer customer)
        {
            var createdCustomerOrder = await _customerOrderService.CreateCustomerOrderAsync(customer, SelectedProduct);

            if (createdCustomerOrder != null)
            {
                NotificationUtility.ShowSuccess("Customer order has been added");
            }
        }
    }
}
