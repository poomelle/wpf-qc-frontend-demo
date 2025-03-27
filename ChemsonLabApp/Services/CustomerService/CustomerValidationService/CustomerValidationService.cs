using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.CustomerValidationService
{
    public class CustomerValidationService : ICustomerValidationService
    {
        private readonly ICustomerRestAPI _customerRestAPI;

        public CustomerValidationService(ICustomerRestAPI customerRestAPI)
        {
            this._customerRestAPI = customerRestAPI;
        }

        private async Task<bool> IsNotDuplicateCustomerAsync(string customerName)
        {
            var customers = await _customerRestAPI.GetAllCustomersAsync();

            var isDuplicate = customers.Any(c => c.name.Equals(customerName, StringComparison.OrdinalIgnoreCase));

            if (isDuplicate)
            {
                NotificationUtility.ShowError($"{customerName} has existed");
            }

            return !isDuplicate;
        }

        private bool IsValidEmail(string email)
        {
            var isValidEmail = true;

            if (!string.IsNullOrWhiteSpace(email))
            {
                isValidEmail = InputValidationUtility.ValidateEmailFormat(email);
            }

            return isValidEmail;
        }

        public async Task<bool> ValidateNewCustomerAsync(string customerName, string email)
        {
            var isNotNullInput = InputValidationUtility.ValidateNotNullInput(customerName, "Customer name") && InputValidationUtility.ValidateNotNullInput(email, "Email");
            var isNotDuplicate = await IsNotDuplicateCustomerAsync(customerName);

            var isEmailFormatValidate = IsValidEmail(email);

            return isNotNullInput && isNotDuplicate && isEmailFormatValidate;
        }

        public async Task<bool> ValidateUpdateCustomerAsync(Customer customer)
        {
            var isNotNullCustomerName = InputValidationUtility.ValidateNotNullInput(customer.name, "Customer name");
            var isEmailFormatValidate = IsValidEmail(customer.email);

            var customers = await _customerRestAPI.GetAllCustomersAsync();

            // exclude the current customer from customerId
            var isNotDuplicate = customers.Any(c => c.name.Equals(customer.name, StringComparison.OrdinalIgnoreCase) && c.id != customer.id);

            return isNotNullCustomerName && isNotDuplicate && isEmailFormatValidate;
        }
    }
}
