using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.CustomerVM.Commands;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.CustomerVM
{
    [AddINotifyPropertyChangedInterface]
    public class DeleteCustomerViewModel
    {
        private readonly ICustomerViewService _customerViewService;

        public string DeleteConfirm { get; set; }
        public Customer Customer { get; set; }
        public DeleteCustomerCommand DeleteCustomerCommand { get; set; }

        public DeleteCustomerViewModel(ICustomerViewService customerViewService)
        {
            // Commands
            DeleteCustomerCommand = new DeleteCustomerCommand(this);

            // services
            this._customerViewService = customerViewService;
        }

        /// <summary>
        /// Deletes the specified customer and their associated orders after confirmation.
        /// Shows a success notification if the deletion is successful.
        /// </summary>
        public async void DeleteCustomer()
        {
            var deletedCustomer = await _customerViewService.DeleteCustomerAndCustomerOrderAsync(Customer, DeleteConfirm);

            if (deletedCustomer != null)
            {
                NotificationUtility.ShowSuccess(deletedCustomer.name + " has been deleted");
            }
        }
    }
}
