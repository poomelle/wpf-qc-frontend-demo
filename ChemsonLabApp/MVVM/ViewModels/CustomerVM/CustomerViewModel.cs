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
using ChemsonLabApp.Services.CustomerValidationService;
using ChemsonLabApp.Services.DialogService;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.CustomerVM
{
    [AddINotifyPropertyChangedInterface]
    public class CustomerViewModel
    {
        private readonly ICustomerViewService _customerService;
        private readonly IDialogService _dialogService;

        //public List<CustomerOrder> CustomerOrders { get; set; } = new List<CustomerOrder>();
        public Customer ComboBoxSelectedCustomer { get; set; }
        public List<Customer> CustomerComboBox { get; set; }
        public ObservableCollection<Customer> Customers { get; set; } = new ObservableCollection<Customer>();
        public List<string> StatusCombo { get; set; } = new List<string> { "All", "Active", "Inactive" };
        public string ComboBoxSelectedStatus { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsLoading { get; set; }
        public bool HasError { get; set; }
        public ReloadCustomerDataCommand ReloadCustomerDataCommand { get; set; }
        public CustomerEditToggleCommand CustomerEditToggleCommand { get; set; }
        public UpdateCustomerCommand UpdateCustomerCommand { get; set; }
        public ShowAddCustomerView ShowAddCustomerView { get; set; }
        public ShowDeleteCustomerViewCommand ShowDeleteCustomerViewCommand { get; set; }
        public CustomerStatusFilterCommand CustomerStatusFilterCommand { get; set; }
        public CustomerNameFilterCommand CustomerNameFilterCommand { get; set; }

        public CustomerViewModel(
            ICustomerViewService customerService, 
            IDialogService dialogService
            )
        {
            // services
            this._customerService = customerService;
            this._dialogService = dialogService;

            // commands
            CustomerEditToggleCommand = new CustomerEditToggleCommand(this);
            ReloadCustomerDataCommand = new ReloadCustomerDataCommand(this);
            UpdateCustomerCommand = new UpdateCustomerCommand(this);
            ShowAddCustomerView = new ShowAddCustomerView(this);
            ShowDeleteCustomerViewCommand = new ShowDeleteCustomerViewCommand(this);
            CustomerStatusFilterCommand = new CustomerStatusFilterCommand(this);
            CustomerNameFilterCommand = new CustomerNameFilterCommand(this);

            // initialize
            InitializeParameter();

        }

        /// <summary>
        /// Initializes customer-related parameters by loading all customers for the ComboBox and populating the Customers collection.
        /// Handles errors related to server connectivity and general exceptions, displaying appropriate notifications.
        /// Shows a loading cursor during the operation.
        /// </summary>
        public async void InitializeParameter()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                CustomerComboBox = await _customerService.GetAllCustomersAsync();
                await GetAllCustomerAsync();
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Failed to connect to server. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("An error occurred while loading data. Please try again.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        /// <summary>
        /// Retrieves all customers asynchronously, applies status and name filters, and updates the Customers collection.
        /// </summary>
        public async Task GetAllCustomerAsync()
        {
            var customers = await _customerService.GetAllCustomersAsync();

            var selectedCustomer = ComboBoxSelectedCustomer == null ? "All" : ComboBoxSelectedCustomer.name;

            var filteredByStatus = _customerService.FilterByStatus(customers, ComboBoxSelectedStatus);
            var filteredByName = _customerService.FilterByName(filteredByStatus, selectedCustomer);

            Customers.Clear();
            if (filteredByName != null)
            {
                foreach (var customer in filteredByName)
                {
                    Customers.Add(customer);
                }
            }
        }

        /// <summary>
        /// Toggles the edit and view modes for the specified customer.
        /// If toggled back to view mode, refreshes the customer list.
        /// </summary>
        public async Task EditViewToggle(Customer customer)
        {
            customer.isEditMode = !customer.isEditMode;
            customer.isViewMode = !customer.isViewMode;

            if (customer.isViewMode)
            {
                await GetAllCustomerAsync();
            }
        }

        /// <summary>
        /// Saves changes made to the specified customer by updating it through the service,
        /// then refreshes the customer list.
        /// </summary>
        public async Task SaveChangeCustomer(Customer customer)
        {
            await _customerService.UpdateCustomerAsync(customer);
            await GetAllCustomerAsync();
        }

        /// <summary>
        /// Reloads the customer data by resetting the status and selected customer filters,
        /// then refreshes the customer list.
        /// </summary>
        public async Task ReloadCustomerData()
        {
            ComboBoxSelectedStatus = "Active";
            ComboBoxSelectedCustomer = null;
            await GetAllCustomerAsync();
        }

        /// <summary>
        /// Opens the Add Customer dialog view using the dialog service.
        /// </summary>
        public void PopupAddCustomerView()
        {
            _dialogService.ShowView("AddCustomer");
        }

        /// <summary>
        /// Opens the Delete Customer dialog view for the specified customer using the dialog service.
        /// </summary>
        /// <param name="customer">The customer to be deleted.</param>
        public void PopupDeleteCustomerView(Customer customer)
        {
            _dialogService.ShowDeleteView(customer);
        }
    }
}
