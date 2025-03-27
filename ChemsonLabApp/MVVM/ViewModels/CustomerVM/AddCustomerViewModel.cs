using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.CustomerVM.Commands;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.CustomerValidationService;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.CustomerVM
{
    [AddINotifyPropertyChangedInterface]
    public class AddCustomerViewModel
    {
        private readonly ICustomerViewService _customerViewService;

        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public SaveCustomerCommand SaveCustomerCommand { get; set; }
        public AddCustomerViewModel(ICustomerViewService customerViewService)
        {
            // commands
            SaveCustomerCommand = new SaveCustomerCommand(this);

            // services
            this._customerViewService = customerViewService;
        }

        public async void SaveNewCustomer()
        {
            var createdCustomer = await _customerViewService.CreateCustomerAsync(CustomerName, CustomerEmail);

            if (createdCustomer != null)
            {
                NotificationUtility.ShowSuccess(CustomerName + " has been added");
            }

        }
    }
}
