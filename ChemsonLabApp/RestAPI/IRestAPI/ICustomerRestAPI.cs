using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI.IRestAPI
{
    public interface ICustomerRestAPI
    {
        Task<List<Customer>> GetAllCustomersAsync(string filter = "", string sort = "");
        Task<Customer> GetCustomerByIdAsync(int id);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<Customer> DeleteCustomerAsync(Customer customer);
    }
}
