using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services
{
    public class CustomerOrderService : ICustomerOrderService
    {
        private readonly ICustomerOrderRestAPI _customerOrderRestAPI;
        private readonly ICustomerRestAPI _customerRestAPI;

        public CustomerOrderService(ICustomerOrderRestAPI customerOrderRestAPI, ICustomerRestAPI customerRestAPI)
        {
            this._customerOrderRestAPI = customerOrderRestAPI;
            this._customerRestAPI = customerRestAPI;
        }

        /// <summary>
        /// Creates a new customer order for the specified customer and product.
        /// Validates that both customer and product are not null before creating the order.
        /// </summary>
        /// <param name="customer">The customer placing the order.</param>
        /// <param name="product">The product being ordered.</param>
        /// <returns>The created <see cref="CustomerOrder"/> if successful; otherwise, null.</returns>
        public async Task<CustomerOrder> CreateCustomerOrderAsync(Customer customer, Product product)
        {
            var isCustomerNotNull = InputValidationUtility.ValidateNotNullObject(customer, "Customer");
            var isProductNotNull = InputValidationUtility.ValidateNotNullObject(product, "Product");

            if (!isCustomerNotNull || !isProductNotNull)
            {
                return null;
            }

            var customerOrder = new CustomerOrder
            {
                customerId = customer.id,
                productId = product.id,
                status = true
            };

            return await _customerOrderRestAPI.CreateCustomerOrderAsync(customerOrder);
        }

        /// <summary>
        /// Deletes the specified customer order if the delete confirmation is valid.
        /// </summary>
        /// <param name="customerOrder">The customer order to delete.</param>
        /// <param name="deleteConfirmation">The confirmation string for deletion.</param>
        /// <returns>The deleted <see cref="CustomerOrder"/> if successful; otherwise, null.</returns>
        public async Task<CustomerOrder> DeleteCustomerOrderAsync(CustomerOrder customerOrder, string deleteConfirmation)
        {
            if (!InputValidationUtility.DeleteConfirmation(deleteConfirmation)) return null;

            return await _customerOrderRestAPI.DeleteCustomerOrderAsync(customerOrder);
        }

        /// <summary>
        /// Retrieves all customer orders, optionally filtered and sorted.
        /// </summary>
        /// <param name="filter">The filter string to apply.</param>
        /// <param name="sort">The sort string to apply.</param>
        /// <returns>A list of <see cref="CustomerOrder"/> objects.</returns>
        public async Task<List<CustomerOrder>> GetAllCustomerOrdersAsync(string filter = "", string sort = "")
        {
            return await _customerOrderRestAPI.GetAllCustomerOrdersAsync(filter, sort);
        }

        /// <summary>
        /// Updates the specified customer information.
        /// </summary>
        /// <param name="customer">The customer to update.</param>
        /// <returns>The updated <see cref="Customer"/> object.</returns>
        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            return await _customerRestAPI.UpdateCustomerAsync(customer);
        }
    }
}
