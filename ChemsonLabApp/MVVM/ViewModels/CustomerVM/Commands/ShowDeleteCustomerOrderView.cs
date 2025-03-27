using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.CustomerVM.Commands
{
    public class ShowDeleteCustomerOrderView : ICommand
    {
        public CustomerOrderViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public ShowDeleteCustomerOrderView(CustomerOrderViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is CustomerOrder customerOrder)
            {
                viewModel.PopupDeleteCustomerOrderView(customerOrder);
            }
        }
    }
}
