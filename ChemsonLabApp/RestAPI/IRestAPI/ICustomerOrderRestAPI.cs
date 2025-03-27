using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI.IRestAPI
{
    public interface ICustomerOrderRestAPI
    {
        Task<List<CustomerOrder>> GetAllCustomerOrdersAsync(string filter = "", string sort = "");
        Task<CustomerOrder> GetCustomerOrderByIdAsync(int id);
        Task<CustomerOrder> CreateCustomerOrderAsync(CustomerOrder customerOrder);
        Task<CustomerOrder> UpdateCustomerOrderAsync(CustomerOrder customerOrder);
        Task<CustomerOrder> DeleteCustomerOrderAsync(CustomerOrder customerOrder);
    }
}
