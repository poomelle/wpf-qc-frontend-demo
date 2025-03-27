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

        public async Task<CustomerOrder> DeleteCustomerOrderAsync(CustomerOrder customerOrder, string deleteConfirmation)
        {
            if (!InputValidationUtility.DeleteConfirmation(deleteConfirmation)) return null;


            return await _customerOrderRestAPI.DeleteCustomerOrderAsync(customerOrder);
        }

        public async Task<List<CustomerOrder>> GetAllCustomerOrdersAsync(string filter = "", string sort = "")
        {
            return await _customerOrderRestAPI.GetAllCustomerOrdersAsync(filter, sort);
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            return await _customerRestAPI.UpdateCustomerAsync(customer);
        }
    }
}
