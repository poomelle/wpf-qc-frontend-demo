using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.CustomerValidationService
{
    public interface ICustomerValidationService
    {
        Task<bool> ValidateNewCustomerAsync(string customerName, string email);
        Task<bool> ValidateUpdateCustomerAsync(Customer customer);
    }
}
