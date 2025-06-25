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

        /// <summary>
        /// Creates a new customer after validating the provided name and email.
        /// </summary>
        /// <param name="customerName">The name of the customer to create.</param>
        /// <param name="email">The email address of the customer.</param>
        /// <returns>The created <see cref="Customer"/> object, or null if validation fails.</returns>
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

        /// <summary>
        /// Deletes the specified customer and all associated customer orders after confirmation.
        /// </summary>
        /// <param name="customer">The customer to delete.</param>
        /// <param name="deleteConfirmation">The confirmation string for deletion.</param>
        /// <returns>The deleted <see cref="Customer"/> object, or null if confirmation fails.</returns>
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

        /// <summary>
        /// Retrieves all customers with optional filtering and sorting.
        /// </summary>
        /// <param name="filter">The filter string to apply.</param>
        /// <param name="sort">The sort string to apply.</param>
        /// <returns>A list of <see cref="Customer"/> objects.</returns>
        public async Task<List<Customer>> GetAllCustomersAsync(string filter = "", string sort = "")
        {
            return await _customerRestAPI.GetAllCustomersAsync(filter, sort);
        }

        /// <summary>
        /// Loads all active customers, sorted by name in ascending order.
        /// </summary>
        /// <returns>A list of active <see cref="Customer"/> objects.</returns>
        public async Task<List<Customer>> LoadActiveCustomers()
        {
            var filter = "?status=true";
            string sort = "&sortBy=Name&isAscending=true";

            return await _customerRestAPI.GetAllCustomersAsync(filter, sort);
        }

        /// <summary>
        /// Updates the specified customer.
        /// </summary>
        /// <param name="customer">The customer to update.</param>
        /// <returns>The updated <see cref="Customer"/> object.</returns>
        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            return await _customerRestAPI.UpdateCustomerAsync(customer);
        }

        /// <summary>
        /// Filters the provided list of customers by the selected customer name.
        /// </summary>
        /// <param name="customers">The list of customers to filter.</param>
        /// <param name="selectedCustomerName">The name to filter by.</param>
        /// <returns>A filtered list of <see cref="Customer"/> objects.</returns>
        public List<Customer> FilterByName(List<Customer> customers, string selectedCustomerName)
        {
            if (string.IsNullOrWhiteSpace(selectedCustomerName) || selectedCustomerName == "All")
                return customers;

            return customers.Where(c => c.name == selectedCustomerName).ToList();
        }

        /// <summary>
        /// Filters the provided list of customers by status ("Active" or "Inactive").
        /// </summary>
        /// <param name="customers">The list of customers to filter.</param>
        /// <param name="status">The status to filter by ("Active", "Inactive", or "All").</param>
        /// <returns>A filtered list of <see cref="Customer"/> objects.</returns>
        public List<Customer> FilterByStatus(List<Customer> customers, string status)
        {
            if (status == "All" || string.IsNullOrWhiteSpace(status))
                return customers;

            bool isActive = status == "Active";
            return customers.Where(c => c.status == isActive).ToList();
        }
    }
}
