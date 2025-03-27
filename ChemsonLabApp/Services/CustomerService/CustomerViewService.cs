using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.CustomerValidationService;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using OfficeOpenXml.VBA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services
{
    public class CustomerViewService : ICustomerViewService
    {
        private readonly ICustomerRestAPI _customerRestAPI;
        private readonly ICustomerOrderRestAPI _customerOrderRestAPI;
        private readonly ICustomerValidationService _customerValidationService;

        public CustomerViewService(ICustomerRestAPI customerRestAPI, ICustomerOrderRestAPI customerOrderRestAPI, ICustomerValidationService customerValidationService)
        {
            this._customerRestAPI = customerRestAPI;
            this._customerOrderRestAPI = customerOrderRestAPI;
            this._customerValidationService = customerValidationService;
        }

        public async Task<Customer> CreateCustomerAsync(string customerName, string email)
        {
            if (!await _customerValidationService.ValidateNewCustomerAsync(customerName, email)) return null;

            var customer = new Customer
            {
                name = customerName,
                email = email,
                status = true
            };

            return await _customerRestAPI.CreateCustomerAsync(customer);
        }

        public async Task<Customer> DeleteCustomerAndCustomerOrderAsync(Customer customer, string deleteConfirmation)
        {
            if (!InputValidationUtility.DeleteConfirmation(deleteConfirmation)) return null;

            string filter = $"?customerName={customer.name}";
            var customerOrders = await _customerOrderRestAPI.GetAllCustomerOrdersAsync(filter);

            if (customerOrders != null || customerOrders.Count != 0)
            {
                foreach (var customerOrder in customerOrders)
                {
                    await _customerOrderRestAPI.DeleteCustomerOrderAsync(customerOrder);
                }
            }

            var deletedCustomer = await _customerRestAPI.DeleteCustomerAsync(customer);

            return deletedCustomer;
        }

        public async Task<List<Customer>> GetAllCustomersAsync(string filter = "", string sort = "")
        {
            return await _customerRestAPI.GetAllCustomersAsync(filter, sort);
        }

        public async Task<List<Customer>> LoadActiveCustomers()
        {
            var filter = "?status=true";
            string sort = "&sortBy=Name&isAscending=true";

            return await _customerRestAPI.GetAllCustomersAsync(filter, sort);
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            return await _customerRestAPI.UpdateCustomerAsync(customer);
        }

        public List<Customer> FilterByName(List<Customer> customers, string selectedCustomerName)
        {
            if (string.IsNullOrWhiteSpace(selectedCustomerName) || selectedCustomerName == "All")
                return customers;

            return customers.Where(c => c.name == selectedCustomerName).ToList();
        }

        public List<Customer> FilterByStatus(List<Customer> customers, string status)
        {
            if (status == "All" || string.IsNullOrWhiteSpace(status))
                return customers;

            bool isActive = status == "Active";
            return customers.Where(c => c.status == isActive).ToList();
        }
    }
}
