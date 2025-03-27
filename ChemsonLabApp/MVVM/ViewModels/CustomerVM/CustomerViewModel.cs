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
        public string ComboBoxSelectedStatus { get; set; } = "Active";
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

        public async Task GetAllCustomerAsync()
        {
            var customers = await _customerService.GetAllCustomersAsync();

            var selectedCustomer = ComboBoxSelectedCustomer == null ? "All" : ComboBoxSelectedCustomer.name;

            var filteredByStatus = _customerService.FilterByStatus(customers, ComboBoxSelectedStatus);
            var filteredByName = _customerService.FilterByName(filteredByStatus, selectedCustomer);

            Customers.Clear();
            if (filteredByName != null)
            {
                foreach(var customer in filteredByName)
                {
                    Customers.Add(customer);
                }
            }
        }

        public async Task EditViewToggle(Customer customer)
        {
            customer.isEditMode = !customer.isEditMode;
            customer.isViewMode = !customer.isViewMode;

            if (customer.isViewMode)
            {
                await GetAllCustomerAsync();
            }
        }

        public async Task SaveChangeCustomer(Customer customer)
        {
            await _customerService.UpdateCustomerAsync(customer);
            await GetAllCustomerAsync();
        }

        public async Task ReloadCustomerData() 
        {
            ComboBoxSelectedStatus = "Active";
            ComboBoxSelectedCustomer = null;
            await GetAllCustomerAsync();
        }

        public void PopupAddCustomerView()
        {
            _dialogService.ShowView("AddCustomer");
        }

        public void PopupDeleteCustomerView(Customer customer)
        {
            _dialogService.ShowDeleteView(customer);
        }
    }
}
