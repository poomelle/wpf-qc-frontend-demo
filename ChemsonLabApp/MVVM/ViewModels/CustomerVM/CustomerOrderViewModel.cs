using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.CustomerVM.Commands;
using ChemsonLabApp.MVVM.Views.Customer;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.DialogService;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.CustomerVM
{
    [AddINotifyPropertyChangedInterface]
    public class CustomerOrderViewModel
    {
        private readonly ICustomerOrderService _customerOrderService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<CustomerOrder> CustomerOrders { get; set; } = new ObservableCollection<CustomerOrder>();
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public Product ComboBoxSelectedProduct { get; set; }
        public Customer ComboBoxSeletedCustomer { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsLoading { get; set; }
        public bool HasError { get; set; }
        public EditViewModeToggleCommand EditViewModeToggleCommand { get; set; }
        public UpdateCustomerOrderCommand UpdateCustomerOrderCommand { get; set; }
        public ReloadDataCommand ReloadDataCommand { get; set; }
        public ShowAddCustomerOrderView ShowAddCustomerOrderView { get; set; }
        public ShowDeleteCustomerOrderView ShowDeleteCustomerOrderView { get; set; }
        public CutomerOrderProductFilterCommand CutomerOrderProductFilterCommand { get; set; }
        public CustomerOrderCustomerFilterCommand CustomerOrderCustomerFilterCommand { get; set; }

        public CustomerOrderViewModel(ICustomerOrderService customerOrderService, IDialogService dialogService)
        {
            //services
            this._customerOrderService = customerOrderService;
            this._dialogService = dialogService;

            //commands
            EditViewModeToggleCommand = new EditViewModeToggleCommand(this);
            UpdateCustomerOrderCommand = new UpdateCustomerOrderCommand(this);
            ReloadDataCommand = new ReloadDataCommand(this);
            ShowAddCustomerOrderView = new ShowAddCustomerOrderView(this);
            ShowDeleteCustomerOrderView = new ShowDeleteCustomerOrderView(this);
            CutomerOrderProductFilterCommand = new CutomerOrderProductFilterCommand(this);
            CustomerOrderCustomerFilterCommand = new CustomerOrderCustomerFilterCommand(this);

            // initialize
            InitializeParameter();
        }

        /// <summary>
        /// Initializes parameters for the CustomerOrderViewModel, including loading all customer orders
        /// with a filter for active status. Handles exceptions and manages the cursor state during the operation.
        /// </summary>
        public async void InitializeParameter()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                string filter = "?status=true";
                await GetAllCustomerOrdersAsync(filter: filter);
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

        /// <summary>
        /// Retrieves all customer orders from the service, applies optional filtering and sorting,
        /// updates the CustomerOrders collection, and refreshes the Customers and Products lists
        /// with unique entries based on the retrieved orders.
        /// </summary>
        public async Task GetAllCustomerOrdersAsync(string filter = "", string sort = "")
        {
            var customerOrders = await _customerOrderService.GetAllCustomerOrdersAsync(filter, sort);

            customerOrders = customerOrders.OrderBy(co => co.product.name).ToList();

            CustomerOrders.Clear();

            foreach (var customerOrder in customerOrders)
            {
                CustomerOrders.Add(customerOrder);
            }

            Customers = customerOrders
                .Select(co => co.customer)
                .GroupBy(customer => customer.id)
                .Select(group => group.First())
                .ToList();
            Products = customerOrders
                .Select(co => co.product)
                .GroupBy(product => product.id)
                .Select(group => group.First())
                .ToList();
        }

        /// <summary>
        /// Saves changes made to a customer's information within a customer order.
        /// Validates the input fields for customer name and email, updates the customer via the service,
        /// refreshes the customer orders list, toggles the edit/view mode for the order, and refreshes the list again.
        /// </summary>
        public async void SaveChangeCustomer(CustomerOrder customerOrder)
        {
            if (!InputValidationUtility.ValidateNotNullInput(customerOrder.customer.name, "Customer Name") &&
                !InputValidationUtility.ValidateNotNullInput(customerOrder.customer.email, "Customer Email")) return;

            Customer customer = new Customer
            {
                id = customerOrder.customer.id,
                name = customerOrder.customer.name,
                email = customerOrder.customer.email,
                status = customerOrder.customer.status,
            };

            await _customerOrderService.UpdateCustomerAsync(customer);

            await GetAllCustomerOrdersAsync();

            EditViewToggle(customerOrder);
            await GetAllCustomerOrdersAsync();
        }

        /// <summary>
        /// Toggles the edit and view modes for a given CustomerOrder.
        /// If switched to view mode, refreshes the list of customer orders from the service.
        /// </summary>
        public async void EditViewToggle(CustomerOrder customerOrder)
        {
            customerOrder.isEditMode = !customerOrder.isEditMode;
            customerOrder.isViewMode = !customerOrder.isViewMode;

            if (customerOrder.isViewMode)
            {
                await GetAllCustomerOrdersAsync();
            }
        }

        /// <summary>
        /// Opens the Add Customer Order dialog view using the dialog service.
        /// </summary>
        public void PopupAddCustomerOrderView()
        {
            _dialogService.ShowView("AddCustomerOrder");
        }

        /// <summary>
        /// Opens the Delete Customer Order dialog view for the specified customer order using the dialog service.
        /// </summary>
        /// <param name="customerOrder">The customer order to be deleted.</param>
        public void PopupDeleteCustomerOrderView(CustomerOrder customerOrder)
        {
            _dialogService.ShowDeleteView(customerOrder);
        }

        /// <summary>
        /// Reloads the customer orders data asynchronously.
        /// </summary>
        public async void ReloadData()
        {
            await GetAllCustomerOrdersAsync();
        }

        /// <summary>
        /// Filters the CustomerOrders collection based on the selected product in ComboBoxSelectedProduct.
        /// If no product is selected, reloads all active customer orders.
        /// Otherwise, sets the 'show' property to false for orders whose product does not match the selected product.
        /// </summary>
        public async void CustomerOrderProductFilter()
        {
            if (ComboBoxSelectedProduct == null)
            {
                await GetAllCustomerOrdersAsync(filter: "?status=true");
            }
            else
            {
                var productId = ComboBoxSelectedProduct.id;
                foreach (var customerOrder in CustomerOrders)
                {
                    if (customerOrder.product.id != productId)
                    {
                        customerOrder.show = false;
                    }
                }
            }
        }

        /// <summary>
        /// Filters the CustomerOrders collection based on the selected customer in ComboBoxSeletedCustomer.
        /// If no customer is selected, reloads all active customer orders.
        /// Otherwise, sets the 'show' property to false for orders whose customer does not match the selected customer.
        /// </summary>
        public async void CustomerOrderCustomerFilter()
        {
            if (ComboBoxSeletedCustomer == null)
            {
                await GetAllCustomerOrdersAsync(filter: "?status=true");
            }
            else
            {
                var customerId = ComboBoxSeletedCustomer.id;
                foreach (var customerOrder in CustomerOrders)
                {
                    if (customerOrder.customer.id != customerId)
                    {
                        customerOrder.show = false;
                    }
                }
            }
        }
    }
}
