using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.IService
{
    public interface ICustomerOrderService
    {
        Task<List<CustomerOrder>> GetAllCustomerOrdersAsync(string filter = "", string sort = "");
        Task<CustomerOrder> CreateCustomerOrderAsync(Customer customer, Product product);
        Task<CustomerOrder> DeleteCustomerOrderAsync(CustomerOrder customerOrder, string deleteConfirmation);
        Task<Customer> UpdateCustomerAsync(Customer customer);
    }
}
