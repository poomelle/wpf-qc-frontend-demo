using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public class DeleteCustomerOrderViewModel
    {
        private readonly ICustomerOrderService _customerOrderService;
        private readonly ICustomerViewService _customerService;

        public string DeleteConfirm { get; set; }
        public CustomerOrder CustomerOrder { get; set; }
        public bool IsDeleteCustomerOrderOnly { get; set; } = true;
        public DeleteCustomerOrderCommand DeleteCustomerOrderCommand { get; set; }
        public DeleteCustomerOrderViewModel(ICustomerOrderService customerOrderService, ICustomerViewService customerService)
        {
            // commands
            DeleteCustomerOrderCommand = new DeleteCustomerOrderCommand(this);

            // services
            this._customerOrderService = customerOrderService;
            this._customerService = customerService;
        }

        /// <summary>
        /// ViewModel responsible for handling the deletion of customer orders and optionally the associated customer.
        /// Provides logic to confirm and execute deletions, and notifies the user upon successful operations.
        /// </summary>
        public async void DeleteCustomerOrder()
        {
            if (IsDeleteCustomerOrderOnly)
            {
                var deletedCustomerOrder = await _customerOrderService.DeleteCustomerOrderAsync(CustomerOrder, DeleteConfirm);
                if (deletedCustomerOrder != null) NotificationUtility.ShowSuccess("The order has been deleted");
            }
            else
            {
                var deletedCustomer = await _customerService.DeleteCustomerAndCustomerOrderAsync(CustomerOrder.customer, DeleteConfirm);
                if (deletedCustomer != null) NotificationUtility.ShowSuccess(deletedCustomer.name + " and all of its orders have been deleted");
            }
        }
    }
}
