using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.IService
{
    public interface ICustomerViewService
    {
        Task<List<Customer>> LoadActiveCustomers();
        Task<List<Customer>> GetAllCustomersAsync(string filter = "", string sort = "");
        Task<Customer> CreateCustomerAsync(string customerName, string email);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<Customer> DeleteCustomerAndCustomerOrderAsync(Customer customer, string deleteConfirmation);
        List<Customer> FilterByStatus(List<Customer> customers, string status);
        List<Customer> FilterByName(List<Customer> customers, string selectedCustomerName);
    }
}
